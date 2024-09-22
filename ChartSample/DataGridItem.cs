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
        private decimal _startFreq;
        private decimal _stopFreq;
        private bool _isSweepFreq = false;
        private decimal _startAmpl;
        private decimal _stopAmpl;
        private bool _isSweepAmpl = false;
        private decimal _startDcOffset;
        private decimal _stopDcOffset;
        private bool _isSweepDcOffset = false;
        private decimal _symmetry;
        private decimal _steptime;

        public event PropertyChangedEventHandler PropertyChanged;

        public string WaveType { get=>_wavetype; set=>value.To(ref _wavetype, onPropertyChanged, nameof(WaveType)); }
        public decimal StartFrequency { get=>_startFreq; set => value.To(ref _startFreq, onPropertyChanged, nameof(StartFrequency)); }
        public decimal StopFrequency { get=>_stopFreq; set => value.To(ref _stopFreq, onPropertyChanged, nameof(StopFrequency)); }
        public bool IsSweepFrequency { get=>_isSweepFreq; set => value.To(ref _isSweepFreq, onPropertyChanged, nameof(IsSweepFrequency)); }
        public decimal StartAmplitude { get=>_startAmpl; set => value.To(ref _startAmpl, onPropertyChanged, nameof(StartAmplitude)); }
        public decimal StopAmplitude { get=>_stopAmpl; set => value.To(ref _stopAmpl, onPropertyChanged, nameof(StopAmplitude)); }
        public bool IsSweepAmplitude { get=>_isSweepAmpl; set => value.To(ref _isSweepAmpl, onPropertyChanged, nameof(IsSweepAmplitude)); }
        public decimal StartDcOffset { get=>_startDcOffset; set => value.To(ref _startDcOffset, onPropertyChanged, nameof(StartDcOffset)); }
        public decimal StopDcOffset { get=>_stopDcOffset; set => value.To(ref _stopDcOffset, onPropertyChanged, nameof(StopDcOffset)); }
        public bool IsSweepDcOffset { get=>_isSweepDcOffset; set => value.To(ref _isSweepDcOffset, onPropertyChanged, nameof(IsSweepDcOffset)); }
        public decimal Symmetry { get=>_symmetry; set => value.To(ref _symmetry, onPropertyChanged, nameof(Symmetry)); }
        public decimal StepTime { get=>_steptime; set => value.To(ref _steptime, onPropertyChanged, nameof(StepTime)); }

        public DataGridItem()
        {

        }

        private void onPropertyChanged(string propertyName)
        {
            Debug.WriteLine(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Method to generate a sine wave (for example, can be extended for other wave types)
        private List<decimal> GenerateBaseWaveform(int samples)
        {
            List<decimal> waveform = new List<decimal>();
            for (int i = 0; i < samples; i++)
            {
                decimal time = i * _steptime;
                decimal angle = 2 * (decimal)Math.PI * time; // Angle for sine wave
                waveform.Add((decimal)Math.Sin((double)angle)); // Add sine wave value
            }
            return waveform;
        }

        // Method to apply frequency sweep
        private List<decimal> ApplyFrequencySweep(List<decimal> waveform, int samples)
        {
            if (_isSweepFreq)
            {
                for (int i = 0; i < samples; i++)
                {
                    decimal t = (decimal)i / samples; // Normalized time (0 to 1)
                    decimal freq = _startFreq + (_stopFreq - _startFreq) * t; // Linear frequency sweep
                    decimal angle = 2 * (decimal)Math.PI * freq * i * _steptime;
                    waveform[i] = (decimal)Math.Sin((double)angle);
                }
            }
            return waveform;
        }

        // Method to apply amplitude sweep
        private List<decimal> ApplyAmplitudeSweep(List<decimal> waveform, int samples)
        {
            if (_isSweepAmpl)
            {
                for (int i = 0; i < samples; i++)
                {
                    decimal t = (decimal)i / samples; // Normalized time (0 to 1)
                    decimal amplitude = _startAmpl + (_stopAmpl - _startAmpl) * t; // Linear amplitude sweep
                    waveform[i] *= amplitude; // Scale waveform by amplitude
                }
            }
            else
            {
                for (int i = 0; i < samples; i++)
                {
                    waveform[i] *= _startAmpl; // Apply constant amplitude
                }
            }
            return waveform;
        }

        // Method to apply DC offset sweep
        private List<decimal> ApplyDcOffsetSweep(List<decimal> waveform, int samples)
        {
            if (_isSweepDcOffset)
            {
                for (int i = 0; i < samples; i++)
                {
                    decimal t = (decimal)i / samples; // Normalized time (0 to 1)
                    decimal offset = _startDcOffset + (_stopDcOffset - _startDcOffset) * t; // Linear DC offset sweep
                    waveform[i] += offset; // Add DC offset to waveform
                }
            }
            else
            {
                for (int i = 0; i < samples; i++)
                {
                    waveform[i] += _startDcOffset; // Apply constant DC offset
                }
            }
            return waveform;
        }

        // Method to generate the waveform with sweeps applied
        public List<decimal> GenerateWaveform(int samples)
        {
            // Step 1: Generate base waveform (e.g., sine wave)
            List<decimal> waveform = GenerateBaseWaveform(samples);

            // Step 2: Apply frequency sweep
            waveform = ApplyFrequencySweep(waveform, samples);

            // Step 3: Apply amplitude sweep
            waveform = ApplyAmplitudeSweep(waveform, samples);

            // Step 4: Apply DC offset sweep
            waveform = ApplyDcOffsetSweep(waveform, samples);

            return waveform;
        }
    }
}
