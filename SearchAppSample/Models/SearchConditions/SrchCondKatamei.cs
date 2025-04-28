using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAppSample.Models.SearchConditions
{
    public class SrchCondKatamei
    {
        public string Katamei1 { get; set; } = string.Empty;

        public FilterOperation FilterOperation1 { get; set; } = FilterOperation.Equals;

        public FilterLogic? FilterLogic1 { get; set; } = null;

        public string Katamei2 { get; set; } = string.Empty;

        public FilterOperation? FilterOperation2 { get; set; } = null;

        public FilterLogic? FilterLogic2 { get; set; } = null;

        public string Katamei3 { get; set; } = string.Empty;

        public FilterOperation? FilterOperation3 { get; set; } = null;

        public DateTime? BeginNoukiDate { get; set; } = null;

        public DateTime? EndNoukiDate { get; set; } = null;

    }
}
