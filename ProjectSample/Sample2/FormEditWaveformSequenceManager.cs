using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class FormEditWaveformSequenceManager
    {
        private readonly Dictionary<int, FormEditWaveformSequence> _openForms = new();
        private readonly EventBus _eventBus;
        private readonly ChartRendererProvider _rendererProvider = new();

        public FormEditWaveformSequenceManager(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void OpenEditor(WaveformSequence sequence)
        {
            if (_openForms.TryGetValue(sequence.Id, out var existingForm))
            {
                existingForm.Focus();
                return;
            }

            var renderer = _rendererProvider.GetRenderer(sequence.SeqType);
            var form = new FormEditWaveformSequence(sequence, renderer, _eventBus);
            _openForms[sequence.Id] = form;

            form.FormClosed += (s, e) =>
            {
                _openForms.Remove(sequence.Id);
            };

            form.Show();
        }
    }

}
