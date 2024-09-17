using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample.WaveFormsSeg
{
    public enum FadeType
    {
        None,
        FadeIn,
        FadeOut
    }

    public enum WaveformType
    {
        Sine,
        Square,
        Triangle,
        Sawtooth
    }

    public enum SweepType
    {
        None,
        Frequency,
        Phase,
        Amplitude,
        DCOffset
    }

    public enum SweepShape
    {
        None,
        Linear,
        Logarithmic
    }
}
