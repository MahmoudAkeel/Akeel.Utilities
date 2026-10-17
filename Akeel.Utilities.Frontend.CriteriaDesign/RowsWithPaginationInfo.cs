using System.Collections.Generic;

namespace Akeel.Utilities.Frontend.CriteriaDesign
{
    public class RowsWithPaginationInfo<TListType>
    {
        public List<TListType> Rows { get; set; }

        public ResponsePaginationInfo PaginationInfo { get; set; }
    }
}
