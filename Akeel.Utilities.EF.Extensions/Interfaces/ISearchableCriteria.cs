using System.Linq;

namespace Akeel.Utilities.EF.Extensions.Interfaces
{
    public interface ISearchableCriteria<TEntity, TFilter>
    {
        IQueryable<TEntity> HandleSearchCriteria(IQueryable<TEntity> rows, TFilter filter, long clientId);
    }


    public interface ISearchableCriteriaSingleTenant<TEntity, TFilter>
    {
        IQueryable<TEntity> HandleSearchCriteria(IQueryable<TEntity> rows, TFilter filter);
    }
}