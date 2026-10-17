using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Akeel.Utilities.Frontend.CriteriaDesign
{
    public class ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum> : GetCriteriaDto<TIncludeRelatedObjectsEnum> where TIncludeRelatedObjectsEnum : Enum
    {
        public new static ListCriteriaDto<TFilter, TOrderEnum, TIncludeRelatedObjectsEnum> Default = new();

        private RequestPaginationInfo _paginationInfo;
        private int pageSize = 25;
        public TFilter Entity { get; set; }

        public List<string> ListOrderOptions { set; get; }
        public RequestPaginationInfo PaginationInfo
        {
            get => _paginationInfo;
            set
            {
                if (value == null || (value?.PageIndex == 0 && value?.PageSize == 0))
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

    }
}
