using System;
using System.Linq.Expressions;

namespace Akeel.Utilities.Frontend.CriteriaDesign
{
    public class OrderColumn<TEntity, TQueryableOrderEnum> where TQueryableOrderEnum : Enum
    {
        public static OrderColumn<TEntity> CreateOrder(TQueryableOrderEnum orderItem,
            Expression<Func<TEntity, object>> lambdaProperty)
        {
            return new OrderColumn<TEntity>(
                orderItem.ToString().EndsWith("Asc") ? OrderType.Asc : OrderType.Desc,
                lambdaProperty);
        }
    }

    public class OrderColumn<TEntity>
    {
        public OrderColumn(OrderType orderType, Expression<Func<TEntity, object>> lambdaProperty)
        {
            OrderType = orderType;
            LambdaProperty = lambdaProperty;
        }

        public Expression<Func<TEntity, object>> LambdaProperty { get; }
        public OrderType OrderType { get; }
    }

    public enum OrderType
    {
        Asc, Desc
    }
}