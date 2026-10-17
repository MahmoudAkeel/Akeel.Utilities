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
            List<OrderColumn<TEntity>> listOrdersOptions = new List<OrderColumn<TEntity>>();

            listOrderEnums.ForEach(order =>
            {
                var result = orderableRange.GetOrderColumn(order);
                if (result != null)
                    listOrdersOptions.Add(result);
            });



            IQueryable<TEntity> castedEntity = entity.AsQueryable();
            if (!listOrdersOptions.Any())
                return castedEntity;
            switch (listOrdersOptions.Count())
            {
                case 0: return castedEntity;
                default:
                    castedEntity = Order(entity, listOrdersOptions.First(), true);
                    foreach (var order in listOrdersOptions.Skip(1))
                    {
                        castedEntity = Order(entity, order, false);
                    }

                    return castedEntity;
            }
        }

        static IQueryable<TEntity> Order<TEntity>(IQueryable<TEntity> list, OrderColumn<TEntity> order, bool isFirstTime)
        {

            switch (isFirstTime)
            {
                case true:
                    return order.OrderType == OrderType.Asc
                        ? list.OrderBy(order.LambdaProperty)
                        : list.OrderByDescending(order.LambdaProperty);

                default:
                    return order.OrderType == OrderType.Asc
                        ? ((IOrderedQueryable<TEntity>)list).ThenBy(order.LambdaProperty)
                        : ((IOrderedQueryable<TEntity>)list).OrderByDescending(order.LambdaProperty);
            }

        }
    }
}
