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
    partial class FormEditWaveformSequence : Form
    {
        private readonly WaveformViewModel _viewModel;
        private readonly WaveformSequence _originalSequence;

        public FormEditWaveformSequence(WaveformSequence sequence, IChartRenderer renderer, EventBus eventBus)
        {
            InitializeComponent();

            _originalSequence = sequence;

            var ucChart1 = new UcChart();
            var ucGrid1 = new UcGrid();
            _viewModel = new WaveformViewModel(ucChart1, ucGrid1, eventBus);
            _viewModel.SetDataSource(CloneSequence(sequence), renderer);

            this.FormClosing += (s, e) =>
            {
                // 編集結果を元データに反映
                _viewModel.ApplyChangesTo(_originalSequence);
                _viewModel.Dispose();
            };
        }

        private WaveformSequence CloneSequence(WaveformSequence source)
        {
            return new WaveformSequence
            {
                Id = source.Id,
                Name = source.Name,
                Pitch = source.Pitch,
                SeqType = source.SeqType,
                WaveformSteps = source.WaveformSteps
                    .Select(step => new WaveformStep
                    {
                        Frequency = step.Frequency,
                        Amplitude = step.Amplitude,
                        Phase = step.Phase,
                        StepTime = step.StepTime
                    }).ToList()
            };
        }
    }

}
