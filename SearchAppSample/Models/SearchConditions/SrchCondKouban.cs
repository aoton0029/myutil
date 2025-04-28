using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAppSample.Models.SearchConditions
{
    public class SrchCondKouban
    {
        public string Kouban { get; set; } = string.Empty;

        public DateTime? BeginNoukiDate { get; set; }

        public DateTime? EndNoukiDate { get; set; }

    }
}
