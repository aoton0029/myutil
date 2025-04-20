using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    public enum SequenceType
    {
        Chuck,
        Dechuck
    }

    class WaveformSequence
    {
        public SequenceType SeqType { get; set; }

        public int Id { get; set; }

        public int Pitch { get; set; }

        public string Name { get; set; }

        public List<WaveformStep> WaveformSteps { get; set; }

        public WaveformSequence()
        {
            
        }
    }
}
