using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class FormEditWaveformManager
    {
        private readonly Dictionary<WaveformSequence, FormEditWaveform> _openForms = new();
        private readonly Form _owner;
        private readonly Action<List<WaveformSequence>> _onSequencesUpdated;

        public FormEditWaveformManager(Form owner, Action<List<WaveformSequence>> onSequencesUpdated)
        {
            _owner = owner;
            _onSequencesUpdated = onSequencesUpdated;
        }

        public void OpenEditor(WaveformSequence sequence, List<WaveformSequence> allSequences)
        {
            if (_openForms.ContainsKey(sequence))
            {
                _openForms[sequence].BringToFront();
                return;
            }

            var form = new FormEditWaveform(sequence);
            form.MdiParent = _owner;
            //form.EditingFinished += (s, updated) =>
            //{
            //    // 更新された sequence を既存のリストに反映（参照型なので通常は不要）
            //    _onSequencesUpdated(allSequences);
            //    _openForms.Remove(sequence);
            //};
            form.FormClosed += (s, e) => _openForms.Remove(sequence);
            _openForms[sequence] = form;
            form.Show();
        }
    }

}
