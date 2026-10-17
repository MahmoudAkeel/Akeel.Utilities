using System.Collections.Generic;

namespace Akeel.Utilities.EF.Extensions.Interfaces
{
    public interface ISelectListWithPagination<TEntity, TDto, TIncludeRelatedObjectsEnum>
    {
        TDto ToDto(TEntity entity, List<TIncludeRelatedObjectsEnum> listRelatedObjectEnums);
    }
}