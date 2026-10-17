using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;
using Akeel.Utilities.EF.Extensions.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests
{
    public sealed class ReleaseSmokeTests
    {
        [Fact]
        public void Packed_assembly_has_stable_version_without_uncommitted_baseline_revision()
        {
            var assembly = typeof(QueryableOrderExtension).Assembly;
            Assert.Equal("3.0.0", assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);
        }

        [Fact]
        public async Task Packed_search_include_and_bounded_pagination_execute_on_PostgreSql()
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            await using (var seed = database.Context())
            {
                seed.Set<OrderingNote>().AddRange(
                    new OrderingNote { Id = 1, RowId = 1, Text = "First note" },
                    new OrderingNote { Id = 2, RowId = 3, Text = "Second note" });
                await seed.SaveChangesAsync();
            }
            await using var context = database.Context();
            var helper = new SmokeHelper();
            var source = context.Rows.AsNoTracking();
            var groupRows = source.DoSearchCriteria(10m, 1L, helper);
            Assert.Equal(new[] { 1, 2, 3, 7 }, await groupRows.OrderBy(row => row.Id).Select(row => row.Id).ToArrayAsync());
            var amountRows = source.DoSearchCriteria(20m, helper);
            Assert.Equal(new[] { 1, 3, 5, 8 }, await amountRows.OrderBy(row => row.Id).Select(row => row.Id).ToArrayAsync());
            var included = await amountRows.IncludeRange(new List<Related> { Related.Notes }, helper)
                .DoOrder(new List<RowOrder> { RowOrder.IdAsc }, new RowOrderHelper())
                .Pagination(new RequestPaginationInfo { PageIndex = 1, PageSize = 2 }).ToListAsync();
            Assert.Equal(new[] { 1, 3 }, included.Select(row => row.Id));
            Assert.All(included, row => Assert.Single(row.Notes));
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [Theory]
        [InlineData("bounded")]
        [InlineData("empty")]
        [InlineData("unbounded")]
        public async Task Packed_existing_async_DTO_helper_materializes_rows_and_totals(string scenario)
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            await using var context = database.Context();
            var helper = new SmokeHelper();
            var rows = context.Rows.AsNoTracking().DoSearchCriteria(scenario == "empty" ? 100m : 10m, helper)
                .DoOrder(new List<RowOrder> { RowOrder.IdAsc }, new RowOrderHelper());
            var pagination = scenario == "unbounded" ? null! : new RequestPaginationInfo { PageIndex = 2, PageSize = 3 };
            var result = await rows.OnSelectListWithPaginationAsync(pagination, new List<Related>(), helper);
            var expected = scenario switch
            {
                "bounded" => new[] { 4, 5, 6 },
                "empty" => new int[0],
                _ => Enumerable.Range(1, 8).ToArray()
            };
            Assert.Equal(expected, result.Rows.Select(row => row.Id));
            Assert.Equal(scenario == "empty" ? 0 : 8, result.PaginationInfo.TotalRowsCount);
            Assert.Equal(scenario switch { "empty" => 0, "bounded" => 3, _ => 1 }, result.PaginationInfo.TotalPagesCount);
        }

        public enum Related { Notes }
        public sealed record SmokeDto(int Id, decimal Amount);

        private sealed class SmokeHelper : ISearchableCriteria<OrderingRow, decimal>,
            ISearchableCriteriaSingleTenant<OrderingRow, decimal>, IIncludableRange<OrderingRow, Related>,
            ISelectListWithPagination<OrderingRow, SmokeDto, Related>
        {
            public IQueryable<OrderingRow> HandleSearchCriteria(IQueryable<OrderingRow> rows, decimal minimum, long clientId)
                => rows.Where(row => row.Group == clientId && row.Amount >= minimum);
            public IQueryable<OrderingRow> HandleSearchCriteria(IQueryable<OrderingRow> rows, decimal minimum)
                => rows.Where(row => row.Amount >= minimum);
            public IQueryable<OrderingRow> HandleIncludeRange(IQueryable<OrderingRow> rows, Related related)
                => rows.Include(row => row.Notes);
            public SmokeDto ToDto(OrderingRow entity, List<Related> related) => new(entity.Id, entity.Amount);
        }
    }
}
