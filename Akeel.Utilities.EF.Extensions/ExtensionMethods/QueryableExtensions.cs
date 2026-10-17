using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Akeel.Utilities.EF.Extensions.ExtensionMethods
{
    public static class QueryableExtensions
    {

        public static IQueryable<T> Pagination<T>(this IQueryable<T> source, RequestPaginationInfo pagination)
        {
            pagination = VerifyPagination(pagination);

            // Mean we need all rows to be returned
            if (pagination.PageIndex == -1 && pagination.PageSize == -1)
                return source;

            return source
                  .Skip(pagination.PageSize * (pagination.PageIndex - 1)).Take(pagination.PageSize);

        }

        //With Interface
        public static IQueryable<T> DoSearchCriteria<T, TFilter>(this IQueryable<T> entity, TFilter filterCriteria, long clientId,
            ISearchableCriteria<T, TFilter> searchableCriteria)
        {
            return searchableCriteria.HandleSearchCriteria(entity, filterCriteria, clientId);
        }

        //With Interface
        public static IQueryable<T> DoSearchCriteria<T, TFilter>(this IQueryable<T> entity, TFilter filterCriteria,
            ISearchableCriteriaSingleTenant<T, TFilter> searchableCriteria)
        {
            return searchableCriteria.HandleSearchCriteria(entity, filterCriteria);
        }

        // V4
        public static async Task<RowsWithPaginationInfo<TDto>> OnSelectListWithPaginationAsync<TDto, TEntity, TIncludeRelatedObjectsEnum>(this IQueryable<TEntity> rows,
            RequestPaginationInfo paginationInfo,
            List<TIncludeRelatedObjectsEnum> listRelatedObjectsEnums,
            ISelectListWithPagination<TEntity, TDto, TIncludeRelatedObjectsEnum> selectListWithPagination)
        {
            var rowsCount = await rows.CountAsync();
            if (rowsCount == 0)
            {
                return new RowsWithPaginationInfo<TDto>()
                {
                    Rows = new List<TDto>(),
                    PaginationInfo = new ResponsePaginationInfo()
                    {
                        TotalRowsCount = 0,
                        TotalPagesCount = 0
                    }

                };
            }

            paginationInfo = VerifyPagination(paginationInfo);

            var currentListRelatedObjects = listRelatedObjectsEnums ??
                                            new List<TIncludeRelatedObjectsEnum>();
            var result = await rows
                .Pagination(paginationInfo)
                .Select(selector: x => selectListWithPagination.ToDto(x, currentListRelatedObjects))
                .ToListAsync();
            return new RowsWithPaginationInfo<TDto>()
            {
                Rows = result,
                PaginationInfo = ToPaginationInfo(rowsCount, paginationInfo.PageSize)
            };

        }


        private static RequestPaginationInfo VerifyPagination(RequestPaginationInfo pagination)
        {
            if (pagination == null || pagination?.PageSize <= 0 || pagination?.PageIndex <= 0)
            {
                return new RequestPaginationInfo()
                {
                    PageSize = -1,
                    PageIndex = -1
                };

            }

            return pagination;

        }

        private static ResponsePaginationInfo ToPaginationInfo(int rowsCount, int pageSize)
        {
            return new ResponsePaginationInfo()
            {
                TotalPagesCount = pageSize <=0  ? 1 : (int)Math.Ceiling(rowsCount / (decimal)pageSize),
                TotalRowsCount = rowsCount
            };
        }
    }

}
