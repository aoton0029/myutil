using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Query.Searchs
{
    public class SearchCriteria
    {
        public string? Keyword { get; set; } // 検索キーワード
        public DateTime? StartDate { get; set; } // 開始日
        public DateTime? EndDate { get; set; } // 終了日
    }
}
