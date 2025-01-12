using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformSample
{
    public enum WaveformType 
    { 
        Sine,
        Square,
        Sawtooth,
    }

    public class WaveformEntry
    {
        public WaveformType WaveType { get; set; } // 例: "Sine", "Square", etc.
        public TimeSpan Duration { get; set; } // 時間(秒)

        public double StartFrequency { get; set; }
        public double EndFrequency { get; set; }
        public bool FrequencySweep { get; set; }

        public double StartAmplitude { get; set; }
        public double EndAmplitude { get; set; }
        public bool AmplitudeSweep { get; set; }

        public double StartDCOffset { get; set; }
        public double EndDCOffset { get; set; }
        public bool DCOffsetSweep { get; set; }
        
    }

}
