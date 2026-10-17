using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Akeel.Utilities.Frontend.CriteriaDesign
{
    public class GetCriteriaDto<TIncludeRelatedObjectsEnum> where TIncludeRelatedObjectsEnum : Enum
    {
        public static GetCriteriaDto<TIncludeRelatedObjectsEnum> Default => new();
        public List<string> ListRelatedObjects { get; set; }


        public GetCriteriaDto<TIncludeRelatedObjectsEnum> AddNewInclude(TIncludeRelatedObjectsEnum includeObject)
        {
            switch (ListRelatedObjects)
            {
                case null:
                    ListRelatedObjects = new List<string>
                    {
                        includeObject.ToString()
                    };
                    break;
                default:
                    if (!ListRelatedObjects.Contains(includeObject.ToString()))
                        ListRelatedObjects.Add(includeObject.ToString());
                    break;
            }

            return this;
        }

        public static GetCriteriaDto<TIncludeRelatedObjectsEnum> AddNewInclude(GetCriteriaDto<TIncludeRelatedObjectsEnum> criteria, TIncludeRelatedObjectsEnum includeObject)
        {
            if (criteria is null)
            {
                return new GetCriteriaDto<TIncludeRelatedObjectsEnum>()
                {
                    ListRelatedObjects = new List<string>
                    {
                        includeObject.ToString()
                    }
                };

            }

            switch (criteria.ListRelatedObjects)
            {
                case null:
                    criteria.ListRelatedObjects = new List<string>
                    {
                        includeObject.ToString()
                    };
                    break;
                default:
                    if (!criteria.ListRelatedObjects.Contains(includeObject.ToString()))
                        criteria.ListRelatedObjects.Add(includeObject.ToString());
                    break;
            }

            return criteria;
        }

    }
}