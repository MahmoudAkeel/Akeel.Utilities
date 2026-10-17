using Akeel.Utilities.EF.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Akeel.Utilities.EF.Extensions.ExtensionMethods
{
    public static class QueryableIncludeExtension
    {
        public static IQueryable<TEntity> IncludeRange<TEntity, TIncludeRelatedObjectsEnum>(this IQueryable<TEntity> entityList,
            IList<TIncludeRelatedObjectsEnum> listRelatedObjects,
            IIncludableRange<TEntity, TIncludeRelatedObjectsEnum> includableRange) where TIncludeRelatedObjectsEnum : Enum
        {
            if (listRelatedObjects == null || !listRelatedObjects.Any())
                return entityList;

            foreach (var relatedObject in listRelatedObjects)
            {
                entityList = includableRange.HandleIncludeRange(entityList, relatedObject);
            }
            return entityList.AsQueryable();
        }
    }
}
