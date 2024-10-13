using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample
{
    public class DataGridItem : INotifyPropertyChanged
    {
        private string _wavetype = "Sine";
        private double _startFreq = 1;
        private double _stopFreq = 1;
        private bool _isSweepFreq = false;
        private double _startAmpl = 1;
        private double _stopAmpl = 1;
        private bool _isSweepAmpl = false;
        private double _startDcOffset = 0;
        private double _stopDcOffset = 0;
        private bool _isSweepDcOffset = false;
        private double _symmetry = 0;
        private double _steptime = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        public string WaveType { get=>_wavetype; set=>value.To(ref _wavetype, onPropertyChanged, nameof(WaveType)); }
        public double StartFrequency { get=>_startFreq; set => value.To(ref _startFreq, onPropertyChanged, nameof(StartFrequency)); }
        public double StopFrequency { get=>_stopFreq; set => value.To(ref _stopFreq, onPropertyChanged, nameof(StopFrequency)); }
        public bool IsSweepFrequency { get=>_isSweepFreq; set => value.To(ref _isSweepFreq, onPropertyChanged, nameof(IsSweepFrequency)); }
        public double StartAmplitude { get=>_startAmpl; set => value.To(ref _startAmpl, onPropertyChanged, nameof(StartAmplitude)); }
        public double StopAmplitude { get=>_stopAmpl; set => value.To(ref _stopAmpl, onPropertyChanged, nameof(StopAmplitude)); }
        public bool IsSweepAmplitude { get=>_isSweepAmpl; set => value.To(ref _isSweepAmpl, onPropertyChanged, nameof(IsSweepAmplitude)); }
        public double StartDcOffset { get=>_startDcOffset; set => value.To(ref _startDcOffset, onPropertyChanged, nameof(StartDcOffset)); }
        public double StopDcOffset { get=>_stopDcOffset; set => value.To(ref _stopDcOffset, onPropertyChanged, nameof(StopDcOffset)); }
        public bool IsSweepDcOffset { get=>_isSweepDcOffset; set => value.To(ref _isSweepDcOffset, onPropertyChanged, nameof(IsSweepDcOffset)); }
        public double Symmetry { get=>_symmetry; set => value.To(ref _symmetry, onPropertyChanged, nameof(Symmetry)); }
        public double StepTime { get=>_steptime; set => value.To(ref _steptime, onPropertyChanged, nameof(StepTime)); }

        public DataGridItem()
        {

        }

        private void onPropertyChanged(string propertyName)
        {
            Debug.WriteLine(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("[{0}]{1}", nameof(WaveType), WaveType));
            sb.AppendLine(string.Format("[{0}]{1}", nameof(StartFrequency), StartFrequency));
            sb.AppendLine(string.Format("[{0}]{1}", nameof(StopFrequency), StopFrequency));
            sb.AppendLine(string.Format("[{0}]{1}", nameof(StartAmplitude), StartAmplitude));
            sb.AppendLine(string.Format("[{0}]{1}", nameof(StopAmplitude), StopAmplitude));
            sb.AppendLine(string.Format("[{0}]{1}", nameof(StartDcOffset), StartDcOffset));
            sb.AppendLine(string.Format("[{0}]{1}", nameof(StopDcOffset), StopDcOffset));
            sb.AppendLine(string.Format("[{0}]{1}", nameof(StepTime), StepTime));
            return sb.ToString();
        }

    }
}
