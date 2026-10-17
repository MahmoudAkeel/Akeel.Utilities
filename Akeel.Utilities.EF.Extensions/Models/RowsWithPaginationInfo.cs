using System.Collections.Generic;

namespace Akeel.Utilities.EF.Extensions.Models
{
    public class RowsWithPaginationInfo<TListType>
    {
        public List<TListType> Rows { get; set; }

        public ResponsePaginationInfo PaginationInfo { get; set; }
    }
}
