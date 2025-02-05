using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Query.Searchs
{
    public interface ISearchService
    {
        DataTable Search(SearchCriteria criteria);
    }

}
