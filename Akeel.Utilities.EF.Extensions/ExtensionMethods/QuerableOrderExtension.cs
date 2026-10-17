using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;
using System.Collections.Generic;
using System.Linq;

namespace Akeel.Utilities.EF.Extensions.ExtensionMethods
{
    public static class QueryableOrderExtension
    {
        public static IQueryable<TEntity> DoOrder<TEntity, TQueryableOrderEnum>(this IQueryable<TEntity> entity, List<TQueryableOrderEnum> listOrderEnums,
            IOrderableRange<TEntity, TQueryableOrderEnum> orderableRange)
        {
            if (listOrderEnums == null || !listOrderEnums.Any())
                return entity;

            var columns = new List<OrderColumn<TEntity>>();
            foreach (var option in listOrderEnums)
            {
                var column = orderableRange.GetOrderColumn(option);
                if (column != null)
                    columns.Add(column);
            }

            IOrderedQueryable<TEntity> ordered = null;
            foreach (var column in columns)
            {
                if (ordered == null)
                {
                    ordered = column.OrderType == OrderType.Asc
                        ? entity.OrderBy(column.LambdaProperty)
                        : entity.OrderByDescending(column.LambdaProperty);
                }
                else
                {
                    ordered = column.OrderType == OrderType.Asc
                        ? ordered.ThenBy(column.LambdaProperty)
                        : ordered.ThenByDescending(column.LambdaProperty);
                }
            }

            return ordered ?? entity;
        }
    }
}
