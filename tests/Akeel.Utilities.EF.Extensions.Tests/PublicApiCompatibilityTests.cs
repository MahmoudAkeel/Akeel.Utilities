using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;
using Xunit;

namespace Akeel.Utilities.EF.Extensions.Tests
{
    public sealed class PublicApiCompatibilityTests
    {
        [Fact]
        public void Existing_query_signatures_and_mutable_default_fields_remain_available()
        {
            var helper = new LegacyHelper();
            var criteria = new ListCriteriaDto<int, RowOrder, Related>
            {
                Entity = 1,
                PaginationInfo = new RequestPaginationInfo { PageIndex = 1, PageSize = 2 },
                ListOrderOptions = [nameof(RowOrder.IdAsc)],
                ListRelatedObjects = []
            };
            var source = OrderingCases.Rows().AsQueryable();
            var rows = source.DoSearchCriteria(criteria.Entity, 7L, helper)
                .DoSearchCriteria(criteria.Entity, helper)
                .DoOrder(criteria.ListOrderOptionsEnum, new RowOrderHelper())
                .IncludeRange(criteria.ListRelatedObjectsEnum, helper)
                .Pagination(criteria.PaginationInfo);
            Assert.Equal(new[] { 1, 2 }, rows.Select(row => row.Id));
            // Compilation proves the old asynchronous signature, not EF translation of ToDto.
            Func<Task<RowsWithPaginationInfo<int>>> oldAsyncCall = () =>
                source.OnSelectListWithPaginationAsync(criteria.PaginationInfo, criteria.ListRelatedObjectsEnum, helper);
            Assert.NotNull(oldAsyncCall);
            Assert.True(typeof(ListCriteriaDto<int, RowOrder, Related>).GetField("Default")!.IsStatic);
            Assert.True(typeof(GetCriteriaDto<Related>).GetProperty("Default")!.GetMethod!.IsStatic);
            Assert.True(typeof(StaticValueDto).GetField("PageSize")!.IsStatic);
            Assert.Same(criteria, criteria.AddNewInclude(Related.None));
            Assert.True(ListCriteriaDto<int, RowOrder, Related>.HasRelatedObject(criteria, Related.None));
        }

        public enum Related { None }

        private sealed class LegacyHelper : ISearchableCriteria<OrderingRow, int>,
            ISearchableCriteriaSingleTenant<OrderingRow, int>, IIncludableRange<OrderingRow, Related>,
            ISelectListWithPagination<OrderingRow, int, Related>
        {
            public IQueryable<OrderingRow> HandleSearchCriteria(IQueryable<OrderingRow> rows, int filter, long clientId) => rows.Where(row => row.Id >= filter);
            public IQueryable<OrderingRow> HandleSearchCriteria(IQueryable<OrderingRow> rows, int filter) => rows.Where(row => row.Id >= filter);
            public IQueryable<OrderingRow> HandleIncludeRange(IQueryable<OrderingRow> rows, Related related) => rows;
            public int ToDto(OrderingRow entity, List<Related> related) => entity.Id;
        }
    }
}
