using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformSample
{
    public interface IWaveformGenerator
    {
        double[] Generate(int length, double frequency, double amplitude);
    }
}
