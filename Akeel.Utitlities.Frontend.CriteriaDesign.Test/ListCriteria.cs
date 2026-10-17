using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akeel.Utilities.Frontend.CriteriaDesign;

namespace Akeel.Utitlities.Frontend.CriteriaDesign.Test
{
    internal class ListCriteria<TFilter> : ListCriteriaDto<TFilter, OrderEnum, IncludeEnum>
    {
        public new ListCriteria<TFilter> AddNewInclude(IncludeEnum includeObject)
        {
            base.AddNewInclude(includeObject);
            return this;
        }

    }
}
