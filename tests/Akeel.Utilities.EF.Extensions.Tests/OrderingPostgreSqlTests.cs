using System.Linq;
using System.Threading.Tasks;
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Akeel.Utilities.EF.Extensions.Tests
{
    public sealed class OrderingPostgreSqlTests
    {
        [Theory]
        [MemberData(nameof(OrderingCases.TwoColumns), MemberType = typeof(OrderingCases))]
        public async Task Two_column_ordering_translates_without_an_adapter(RowOrder[] orders, int[] expected)
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            await using var context = database.Context();
            var rows = await context.Rows.AsNoTracking().DoOrder(orders.ToList(), new RowOrderHelper()).ToListAsync();
            Assert.Equal(expected, rows.Select(row => row.Id));
        }

        [Theory]
        [MemberData(nameof(OrderingCases.ThreeColumns), MemberType = typeof(OrderingCases))]
        public async Task Three_column_ordering_translates_without_an_adapter(RowOrder[] orders, int[] expected)
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            await using var context = database.Context();
            var rows = await context.Rows.AsNoTracking().DoOrder(orders.ToList(), new RowOrderHelper()).ToListAsync();
            Assert.Equal(expected, rows.Select(row => row.Id));
        }

        [Theory]
        [InlineData("null")]
        [InlineData("empty")]
        [InlineData("skipped")]
        [InlineData("interleaved")]
        [InlineData("single")]
        public async Task Legacy_edge_cases_execute_on_PostgreSql(string scenario)
        {
            await using var database = await OwnedPostgreSqlDatabase.CreateAsync();
            await using var context = database.Context();
            var source = context.Rows.AsNoTracking();
            var query = source.DoOrder(OrderingCases.EdgeOrders(scenario)!, new RowOrderHelper());
            var ids = (await query.ToListAsync()).Select(row => row.Id).ToArray();
            OrderingCases.AssertResult(scenario, source, query, ids);
        }
    }
}
