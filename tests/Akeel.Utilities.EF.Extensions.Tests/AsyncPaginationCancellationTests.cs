using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using Xunit.Abstractions;

namespace Akeel.Utilities.EF.Extensions.Tests
{
    public sealed class AsyncPaginationCancellationTests(ITestOutputHelper output)
    {
        [Theory]
        [InlineData("bounded")]
        [InlineData("empty")]
        [InlineData("null")]
        [InlineData("zero-index")]
        [InlineData("zero-size")]
        [InlineData("negative-index")]
        [InlineData("negative-size")]
        public async Task Legacy_None_and_live_token_calls_preserve_rows_totals_and_sequential_commands(string scenario)
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            var trace = new CommandTrace();
            await using var context = database.Context(trace);
            var rows = context.Rows.AsNoTracking().OrderBy(row => row.Id).AsQueryable();
            if (scenario == "empty") rows = rows.Where(row => row.Id > 99);
            var pagination = PaginationFor(scenario);
            var helper = new DtoHelper();

            var legacy = await rows.OnSelectListWithPaginationAsync(pagination!, new List<Related>(), helper);
            AssertTrace(trace, CancellationToken.None, scenario == "empty");
            trace.Clear();
            var none = await rows.OnSelectListWithPaginationAsync(pagination!, new List<Related>(), helper, CancellationToken.None);
            AssertTrace(trace, CancellationToken.None, scenario == "empty");
            trace.Clear();
            using var cancellation = new CancellationTokenSource();
            var live = await rows.OnSelectListWithPaginationAsync(pagination!, new List<Related>(), helper, cancellation.Token);
            AssertTrace(trace, cancellation.Token, scenario == "empty");

            var expected = scenario switch
            {
                "bounded" => new[] { 4, 5, 6 },
                "empty" => new int[0],
                _ => Enumerable.Range(1, 8).ToArray()
            };
            Assert.Equal(expected, legacy.Rows.Select(row => row.Id));
            Assert.Equal(legacy.Rows, none.Rows);
            Assert.Equal(legacy.Rows, live.Rows);
            Assert.Equal(scenario == "empty" ? 0 : 8, legacy.PaginationInfo.TotalRowsCount);
            Assert.Equal(scenario switch { "bounded" => 3, "empty" => 0, _ => 1 }, legacy.PaginationInfo.TotalPagesCount);
            Assert.Equal(legacy.PaginationInfo.TotalRowsCount, none.PaginationInfo.TotalRowsCount);
            Assert.Equal(legacy.PaginationInfo.TotalRowsCount, live.PaginationInfo.TotalRowsCount);
            Assert.Equal(legacy.PaginationInfo.TotalPagesCount, none.PaginationInfo.TotalPagesCount);
            Assert.Equal(legacy.PaginationInfo.TotalPagesCount, live.PaginationInfo.TotalPagesCount);
        }

        [Fact]
        public async Task Pre_cancelled_token_is_not_converted_to_success()
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            await using var context = database.Context();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                context.Rows.OnSelectListWithPaginationAsync(
                    new RequestPaginationInfo { PageIndex = 1, PageSize = 2 },
                    new List<Related>(), new DtoHelper(), cancellation.Token));
            Assert.Equal(8, await context.Rows.CountAsync());
        }

        [Fact]
        public async Task Active_count_cancellation_never_starts_materialization_and_context_recovers()
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            var trace = new CommandTrace();
            await using var helperContext = database.Context(trace);
            await using var blockerContext = database.Context();
            await using var observerContext = database.Context();
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            await helperContext.Database.OpenConnectionAsync(deadline.Token);
            await blockerContext.Database.OpenConnectionAsync(deadline.Token);
            await observerContext.Database.OpenConnectionAsync(deadline.Token);
            var helperPid = await BackendPidAsync(helperContext.Database.GetDbConnection(), deadline.Token);
            var blockerPid = await BackendPidAsync(blockerContext.Database.GetDbConnection(), deadline.Token);
            await using var transaction = await blockerContext.Database.BeginTransactionAsync(deadline.Token);
            await blockerContext.Database.ExecuteSqlRawAsync("LOCK TABLE \"Rows\" IN ACCESS EXCLUSIVE MODE", deadline.Token);
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            Task<RowsWithPaginationInfo<RowDto>>? pending = null;
            try
            {
                pending = StartPageAsync(helperContext, cancellation.Token);
                await WaitForExactBlockAsync(observerContext.Database.GetDbConnection(), helperPid, blockerPid, deadline.Token);
                output.WriteLine($"Count observed active and blocked: helper PID {helperPid}, blocker PID {blockerPid}.");
                cancellation.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending.WaitAsync(deadline.Token));
                Assert.Equal(new[] { "count:start" }, trace.Events);
                Assert.Equal(cancellation.Token, Assert.Single(trace.Tokens));
            }
            finally
            {
                await CancelDrainAndRollbackAsync(pending, cancellation, transaction);
            }
            Assert.Equal(8, await helperContext.Rows.CountAsync(deadline.Token));
        }

        [Fact]
        public async Task Active_materialization_cancellation_occurs_after_count_and_context_recovers()
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            var trace = new CommandTrace();
            await using var helperContext = database.Context(trace);
            await using var blockerContext = database.Context();
            await using var observerContext = database.Context();
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            await helperContext.Database.OpenConnectionAsync(deadline.Token);
            await blockerContext.Database.OpenConnectionAsync(deadline.Token);
            await observerContext.Database.OpenConnectionAsync(deadline.Token);
            var helperPid = await BackendPidAsync(helperContext.Database.GetDbConnection(), deadline.Token);
            var blockerPid = await BackendPidAsync(blockerContext.Database.GetDbConnection(), deadline.Token);
            await using var transaction = await blockerContext.Database.BeginTransactionAsync(deadline.Token);
            trace.BeforeMaterialization = async () =>
                await blockerContext.Database.ExecuteSqlRawAsync("LOCK TABLE \"Rows\" IN ACCESS EXCLUSIVE MODE", deadline.Token);
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            Task<RowsWithPaginationInfo<RowDto>>? pending = null;
            try
            {
                pending = StartPageAsync(helperContext, cancellation.Token);
                await WaitForExactBlockAsync(observerContext.Database.GetDbConnection(), helperPid, blockerPid, deadline.Token);
                output.WriteLine($"Materialization observed active and blocked after count: helper PID {helperPid}, blocker PID {blockerPid}.");
                cancellation.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending.WaitAsync(deadline.Token));
                Assert.Equal(new[] { "count:start", "count:complete", "materialize:start" }, trace.Events);
                Assert.Equal(2, trace.Tokens.Count);
                Assert.All(trace.Tokens, token => Assert.Equal(cancellation.Token, token));
            }
            finally
            {
                await CancelDrainAndRollbackAsync(pending, cancellation, transaction);
            }
            Assert.Equal(8, await helperContext.Rows.CountAsync(deadline.Token));
        }

        private static Task<RowsWithPaginationInfo<RowDto>> StartPageAsync(OrderingDbContext context, CancellationToken token)
            => context.Rows.AsNoTracking().OrderBy(row => row.Id).OnSelectListWithPaginationAsync(
                new RequestPaginationInfo { PageIndex = 2, PageSize = 3 }, new List<Related>(), new DtoHelper(), token);

        private static RequestPaginationInfo? PaginationFor(string scenario) => scenario switch
        {
            "null" => null,
            "zero-index" => new RequestPaginationInfo { PageIndex = 0, PageSize = 3 },
            "zero-size" => new RequestPaginationInfo { PageIndex = 2, PageSize = 0 },
            "negative-index" => new RequestPaginationInfo { PageIndex = -1, PageSize = 3 },
            "negative-size" => new RequestPaginationInfo { PageIndex = 1, PageSize = -1 },
            _ => new RequestPaginationInfo { PageIndex = 2, PageSize = 3 }
        };

        private static void AssertTrace(CommandTrace trace, CancellationToken token, bool empty)
        {
            var expected = empty
                ? new[] { "count:start", "count:complete" }
                : new[] { "count:start", "count:complete", "materialize:start", "materialize:complete" };
            Assert.Equal(expected, trace.Events);
            Assert.Equal(empty ? 1 : 2, trace.Tokens.Count);
            Assert.All(trace.Tokens, observed => Assert.Equal(token, observed));
        }

        private static async Task<int> BackendPidAsync(DbConnection connection, CancellationToken token)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT pg_backend_pid()";
            return (int)(await command.ExecuteScalarAsync(token))!;
        }

        private static async Task WaitForExactBlockAsync(DbConnection connection, int helperPid, int blockerPid, CancellationToken token)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT EXISTS (
                    SELECT 1 FROM pg_stat_activity
                    WHERE pid = @helper AND state = 'active' AND wait_event_type = 'Lock'
                    AND @blocker = ANY(pg_blocking_pids(pid)))
                """;
            var helperParameter = command.CreateParameter();
            helperParameter.ParameterName = "helper";
            helperParameter.Value = helperPid;
            command.Parameters.Add(helperParameter);
            var blockerParameter = command.CreateParameter();
            blockerParameter.ParameterName = "blocker";
            blockerParameter.Value = blockerPid;
            command.Parameters.Add(blockerParameter);
            while (!(bool)(await command.ExecuteScalarAsync(token))!)
            {
                await Task.Delay(25, token);
            }
        }

        private static async Task CancelDrainAndRollbackAsync(Task? pending, CancellationTokenSource cancellation, IDbContextTransaction transaction)
        {
            cancellation.Cancel();
            try
            {
                if (pending is not null)
                {
                    try { await pending.WaitAsync(TimeSpan.FromSeconds(5)); }
                    catch (OperationCanceledException) { }
                }
            }
            finally
            {
                await transaction.RollbackAsync().WaitAsync(TimeSpan.FromSeconds(5));
            }
        }

        private enum Related { None }
        private sealed record RowDto(int Id, decimal Amount);
        private sealed class DtoHelper : ISelectListWithPagination<OrderingRow, RowDto, Related>
        {
            public RowDto ToDto(OrderingRow entity, List<Related> related) => new(entity.Id, entity.Amount);
        }

        private sealed class CommandTrace : DbCommandInterceptor
        {
            public List<string> Events { get; } = [];
            public List<CancellationToken> Tokens { get; } = [];
            public Func<Task>? BeforeMaterialization { get; set; }

            public void Clear()
            {
                Events.Clear();
                Tokens.Clear();
            }

            public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                var kind = Kind(command);
                Events.Add(kind + ":start");
                Tokens.Add(cancellationToken);
                if (kind == "materialize" && BeforeMaterialization is not null)
                    await BeforeMaterialization();
                return result;
            }

            public override ValueTask<DbDataReader> ReaderExecutedAsync(
                DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
                CancellationToken cancellationToken = default)
            {
                Events.Add(Kind(command) + ":complete");
                return ValueTask.FromResult(result);
            }

            private static string Kind(DbCommand command)
                => command.CommandText.Contains("count(", StringComparison.OrdinalIgnoreCase) ? "count" : "materialize";
        }
    }
}
