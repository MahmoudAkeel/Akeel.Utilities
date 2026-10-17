using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Akeel.Utilities.EF.Extensions.Models
{
    public class ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum> : GetCriteriaDto<TIncludeRelatedObjectsEnum> where TIncludeRelatedObjectsEnum : Enum
    {
        public new static ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum> Default = new();

        private RequestPaginationInfo _paginationInfo;
        private int pageSize = 25;
        public TFilter Entity { get; set; }
        [JsonIgnore]
        public List<TOrderEnum> ListOrderOptionsEnum => GetEquivalentEnum(ListOrderOptions);
        public List<string> ListOrderOptions { set; get; }
        public RequestPaginationInfo PaginationInfo
        {
            get => _paginationInfo;
            set
            {
                if (value == null || value?.PageIndex == 0 && value?.PageSize == 0)
                {
                    _paginationInfo = new RequestPaginationInfo()
                    {
                        PageSize = pageSize,
                        PageIndex = 1
                    };
                }


                _paginationInfo = value;
            }
        }

        public static ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum> AddNewInclude(ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum> criteria, TIncludeRelatedObjectsEnum includeObject)
        {
            if (criteria is null)
            {
                return new ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum>()
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

        public ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum> AddNewInclude(TIncludeRelatedObjectsEnum includeObject)
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
                    if (ListRelatedObjects.Contains(includeObject.ToString()) == false)
                        ListRelatedObjects.Add(includeObject.ToString());
                    break;
            }

            return this;
        }
        public static bool HasRelatedObject(ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum> criteria, TIncludeRelatedObjectsEnum includeRelated)
        {
            if (criteria is null || criteria.ListRelatedObjectsEnum is null)
                return false;

            return criteria.ListRelatedObjectsEnum.Contains(includeRelated);
        }
        private List<TOrderEnum> GetEquivalentEnum(List<string> list)
        {
            if (list == null || !list.Any())
                return new List<TOrderEnum>();
            List<TOrderEnum> result = new List<TOrderEnum>();

            foreach (string str in list)
            {
                if (Enum.TryParse(typeof(TOrderEnum), str, true, out var currentValue))
                {
                    result.Add((TOrderEnum)currentValue);
                }
            }

            return result;
        }
    }
}
