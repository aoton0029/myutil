using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectSample.Sample2
{
    partial class FormEditWaveform: Form
    {
        WaveformViewModel _vmWaveform;

        public FormEditWaveform(WaveformSequence sequence)
        {
            InitializeComponent();
        }
    }
}
