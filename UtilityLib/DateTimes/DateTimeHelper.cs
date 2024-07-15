using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Helpers
{
    internal class DateTimeHelper
    {

    }

    public class DateTimeRange
    {

        DateTime Start { get; set; }

        DateTime End { get; set; }

        public double TotalMinutes { get { return (this.End - this.Start).TotalMinutes; } }

        public bool IsValid(DateTime target)
        {
            return (target >= Start) && (target <= End);
        }
    }

}
