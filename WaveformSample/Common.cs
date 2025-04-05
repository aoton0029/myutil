using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample
{
    public class Common
    {
        public enum WaveformType
        {
            Sine,
            Square,
            Triangle,
            Sawtooth,
            DC
        }

        public enum SweepType
        {
            None,
            Frequency,
            Phase,
            Amplitude,
            DcOffset
        }

        public enum SweepShape
        {
            None,
            Linear,
            Logarithmic
        }
    }
}
