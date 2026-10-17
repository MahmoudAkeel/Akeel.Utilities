using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Akeel.Utilities.EF.Extensions.Models
{
    public class GetCriteriaDto<TIncludeRelatedObjectsEnum> where TIncludeRelatedObjectsEnum : Enum
    {
        public static GetCriteriaDto<TIncludeRelatedObjectsEnum> Default => new();
        public List<string> ListRelatedObjects { get; set; }
        [JsonIgnore]
        public List<TIncludeRelatedObjectsEnum> ListRelatedObjectsEnum =>
            GetEquivalentEnum(ListRelatedObjects);

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

        public static bool HasRelatedObject(GetCriteriaDto<TIncludeRelatedObjectsEnum> criteria, TIncludeRelatedObjectsEnum includeRelated)
        {
            if (criteria is null || criteria.ListRelatedObjectsEnum is null)
                return false;

            return criteria.ListRelatedObjectsEnum.Contains(includeRelated);
        }

        private List<TIncludeRelatedObjectsEnum> GetEquivalentEnum(IList<string> list)
        {
            if (list == null || !list.Any())
                return new List<TIncludeRelatedObjectsEnum>();
            List<TIncludeRelatedObjectsEnum> result = new();

            foreach (string str in list)
            {
                if (Enum.TryParse(typeof(TIncludeRelatedObjectsEnum), str, true, out var currentValue))
                {
                    result.Add((TIncludeRelatedObjectsEnum)Enum.Parse(typeof(TIncludeRelatedObjectsEnum), currentValue.ToString(), true));
                }
            }

            return result;
        }

    }
}