using Akeel.Utilities.EF.Extensions.Models;

namespace Akeel.Utilities.EF.Extensions.Interfaces
{
    public interface IOrderableRange<TEntity, TQueryableOrderEnum>
    {
        OrderColumn<TEntity> GetOrderColumn(TQueryableOrderEnum order);
    }
}