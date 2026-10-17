using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Akeel.Utilities.EF.Extensions.Tests
{
    public sealed class PaginationTests(ITestOutputHelper output)
    {
        [Theory]
        [InlineData(1, 3, 8, new[] { 1, 2, 3 }, 8L, 3L)]
        [InlineData(2, 3, 8, new[] { 4, 5, 6 }, 8L, 3L)]
        [InlineData(3, 3, 8, new[] { 7, 8 }, 8L, 3L)]
        [InlineData(4, 3, 8, new int[0], 8L, 3L)]
        [InlineData(1, 3, 0, new int[0], 0L, 0L)]
        [InlineData(3, 2, 5, new[] { 5 }, 5L, 3L)]
        [InlineData(2, int.MaxValue, 8, new int[0], 8L, 1L)]
        public async Task PostgreSql_pages_keep_filtered_long_totals_and_sequential_commands(
            int index, int size, int maximumId, int[] ids, long totalRows, long totalPages)
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            var trace = new PaginationTrace();
            await using var context = database.Context(trace);
            var rows = context.Rows.AsNoTracking().Where(row => row.Id <= maximumId).OrderBy(row => row.Id);
            var pagination = new RequestPaginationInfo { PageIndex = index, PageSize = size };

            var legacy = await rows.OnSelectListWithPaginationAsync(pagination, new List<Related>(), new DtoHelper());
            AssertResult(legacy, ids, totalRows, totalPages);
            AssertProviderTrace(trace, totalRows == 0);
            trace.Clear();
            var tokenResult = await rows.OnSelectListWithPaginationAsync(
                pagination, new List<Related>(), new DtoHelper(), CancellationToken.None);
            AssertResult(tokenResult, ids, totalRows, totalPages);
            AssertProviderTrace(trace, totalRows == 0);
            output.WriteLine("PostgreSQL count reader: Int64; count completed before materialization.");
        }

        [Fact]
        public async Task Null_and_any_nonpositive_component_stay_unbounded_before_offset_math()
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            await using var context = database.Context();
            var rows = context.Rows.AsNoTracking().OrderBy(row => row.Id);
            var values = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            var cases = new List<RequestPaginationInfo?> { null };
            foreach (var index in values)
                foreach (var size in values)
                    if (index <= 0 || size <= 0)
                        cases.Add(new RequestPaginationInfo { PageIndex = index, PageSize = size });

            foreach (var pagination in cases)
            {
                Assert.Same(rows, rows.Pagination(pagination!));
                var legacy = await rows.OnSelectListWithPaginationAsync(pagination!, new List<Related>(), new DtoHelper());
                var tokenResult = await rows.OnSelectListWithPaginationAsync(
                    pagination!, new List<Related>(), new DtoHelper(), CancellationToken.None);
                AssertResult(legacy, Enumerable.Range(1, 8).ToArray(), 8, 1);
                AssertResult(tokenResult, Enumerable.Range(1, 8).ToArray(), 8, 1);
            }
            Assert.Equal(22, cases.Count);
        }

        [Theory]
        [InlineData(3, 1073741824, 2147483648L)]
        [InlineData(int.MaxValue, int.MaxValue, 4611686011984936962L)]
        [InlineData(int.MaxValue, 2, 4294967292L)]
        public async Task Oversized_offsets_fail_before_SQL_even_for_empty_queries_and_cancelled_tokens(
            int index, int size, long offset)
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            var trace = new PaginationTrace();
            await using var context = database.Context(trace);
            var pagination = new RequestPaginationInfo { PageIndex = index, PageSize = size };
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            foreach (var empty in new[] { false, true })
            {
                var rows = context.Rows.AsNoTracking().Where(row => !empty || row.Id < 0);
                AssertOffsetException(Assert.Throws<ArgumentOutOfRangeException>(() => rows.Pagination(pagination)), offset);

                var legacy = rows.OnSelectListWithPaginationAsync(pagination, new List<Related>(), new DtoHelper());
                AssertOffsetException(await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => legacy), offset);
                var withToken = rows.OnSelectListWithPaginationAsync(
                    pagination, new List<Related>(), new DtoHelper(), CancellationToken.None);
                AssertOffsetException(await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => withToken), offset);
                var invalidAndCancelled = rows.OnSelectListWithPaginationAsync(
                    pagination, new List<Related>(), new DtoHelper(), cancelled.Token);
                AssertOffsetException(await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => invalidAndCancelled), offset);
            }
            Assert.Empty(trace.Events);
            Assert.Empty(trace.Commands);
        }

        public static IEnumerable<object[]> LongPageCases()
        {
            yield return new object[] { 0L, 2, 0L };
            yield return new object[] { 0L, 0, 0L };
            yield return new object[] { 1L, 0, 1L };
            yield return new object[] { 2147483648L, 2, 1073741824L };
            yield return new object[] { 2147483649L, 2, 1073741825L };
            yield return new object[] { long.MaxValue, 1, long.MaxValue };
            yield return new object[] { long.MaxValue, 2, 4611686018427387904L };
            yield return new object[] { long.MaxValue - 1, 2, 4611686018427387903L };
            yield return new object[] { long.MaxValue, int.MaxValue, 4294967299L };
            yield return new object[] { long.MaxValue - 1, int.MaxValue, 4294967298L };
            yield return new object[] { long.MaxValue, -1, 1L };
            yield return new object[] { long.MaxValue, int.MinValue, 1L };
        }

        [Theory]
        [MemberData(nameof(LongPageCases))]
        public void Synthetic_long_totals_use_safe_integer_page_arithmetic(long count, int size, long expectedPages)
        {
            var method = typeof(QueryableExtensions).GetMethod("ToPaginationInfo", BindingFlags.NonPublic | BindingFlags.Static)!;
            var result = Assert.IsType<ResponsePaginationInfo>(method.Invoke(null, new object[] { count, size }));
            Assert.Equal(count, result.TotalRowsCount);
            Assert.Equal(expectedPages, result.TotalPagesCount);
        }

        private static void AssertOffsetException(ArgumentOutOfRangeException exception, long offset)
        {
            Assert.Equal("pagination", exception.ParamName);
            Assert.Equal(offset, Assert.IsType<long>(exception.ActualValue));
            Assert.Contains("Int32.MaxValue", exception.Message);
        }

        private static void AssertResult(RowsWithPaginationInfo<RowDto> result, int[] ids, long count, long pages)
        {
            Assert.Equal(ids, result.Rows.Select(row => row.Id));
            Assert.Equal(count, result.PaginationInfo.TotalRowsCount);
            Assert.Equal(pages, result.PaginationInfo.TotalPagesCount);
        }

        private static void AssertProviderTrace(PaginationTrace trace, bool empty)
        {
            Assert.Equal(empty
                ? new[] { "count:start", "count:complete" }
                : new[] { "count:start", "count:complete", "materialize:start", "materialize:complete" }, trace.Events);
            Assert.Equal(typeof(long), Assert.Single(trace.CountFieldTypes));
            Assert.False(trace.Commands[0].Contains("LIMIT", StringComparison.OrdinalIgnoreCase));
            Assert.False(trace.Commands[0].Contains("OFFSET", StringComparison.OrdinalIgnoreCase));
        }

        private enum Related { None }
        private sealed record RowDto(int Id);
        private sealed class DtoHelper : ISelectListWithPagination<OrderingRow, RowDto, Related>
        {
            public RowDto ToDto(OrderingRow row, List<Related> related) => new(row.Id);
        }

        private sealed class PaginationTrace : DbCommandInterceptor
        {
            public List<string> Events { get; } = [];
            public List<string> Commands { get; } = [];
            public List<Type> CountFieldTypes { get; } = [];

            public void Clear()
            {
                Events.Clear();
                Commands.Clear();
                CountFieldTypes.Clear();
            }

            public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                Events.Add(Kind(command) + ":start");
                Commands.Add(command.CommandText);
                return ValueTask.FromResult(result);
            }

            public override ValueTask<DbDataReader> ReaderExecutedAsync(
                DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
                CancellationToken cancellationToken = default)
            {
                var kind = Kind(command);
                Events.Add(kind + ":complete");
                if (kind == "count") CountFieldTypes.Add(result.GetFieldType(0));
                return ValueTask.FromResult(result);
            }

            private static string Kind(DbCommand command)
                => command.CommandText.Contains("count(", StringComparison.OrdinalIgnoreCase) ? "count" : "materialize";
        }
    }
}
