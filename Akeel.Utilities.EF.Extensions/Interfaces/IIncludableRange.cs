using System;
using System.Linq;

namespace Akeel.Utilities.EF.Extensions.Interfaces
{
    public interface IIncludableRange<TEntity, TIncludeRelatedObjectsEnum> where TIncludeRelatedObjectsEnum : Enum
    {
        IQueryable<TEntity> HandleIncludeRange(IQueryable<TEntity> rows, TIncludeRelatedObjectsEnum includeEnum);
    }
}
