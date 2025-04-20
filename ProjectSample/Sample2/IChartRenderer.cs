using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    interface IChartRenderer
    {
        void Render(UcChart chart, int pitch, List<WaveformStep> waveformSteps);
    }
}
