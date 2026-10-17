using System.Linq;
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Xunit;

namespace Akeel.Utilities.EF.Extensions.Tests
{
    public sealed class OrderingLinqTests
    {
        [Theory]
        [MemberData(nameof(OrderingCases.TwoColumns), MemberType = typeof(OrderingCases))]
        public void Two_column_ordering_preserves_precedence(RowOrder[] orders, int[] expected)
            => Assert.Equal(expected, OrderingCases.Rows().AsQueryable().DoOrder(orders.ToList(), new RowOrderHelper()).Select(row => row.Id));

        [Theory]
        [MemberData(nameof(OrderingCases.ThreeColumns), MemberType = typeof(OrderingCases))]
        public void Three_column_ordering_preserves_precedence(RowOrder[] orders, int[] expected)
            => Assert.Equal(expected, OrderingCases.Rows().AsQueryable().DoOrder(orders.ToList(), new RowOrderHelper()).Select(row => row.Id));

        [Theory]
        [InlineData("null")]
        [InlineData("empty")]
        [InlineData("skipped")]
        [InlineData("interleaved")]
        [InlineData("single")]
        public void Legacy_edge_cases_remain_compatible(string scenario)
        {
            var source = OrderingCases.Rows().AsQueryable();
            var query = source.DoOrder(OrderingCases.EdgeOrders(scenario)!, new RowOrderHelper());
            OrderingCases.AssertResult(scenario, source, query, query.Select(row => row.Id).ToArray());
        }
    }
}
