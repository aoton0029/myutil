using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample.Models
{
    public class ChartItem
    {
        public double Time { get; set; }

        public double Value { get; set; }

        public double RMS { get; set; }

        public ChartItem() { }

        public ChartItem(double time, double value) 
        {
            Time = time;
            Value = value;
        }
    }
}
