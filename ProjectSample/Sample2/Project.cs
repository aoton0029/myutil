using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class Project
    {
        public string ProjectName { get; set; }

        public string Author { get; set; }

        public List<WaveformSequence> WaveformSequences { get; set; }
    }
}
