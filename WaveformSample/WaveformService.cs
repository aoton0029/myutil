using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformSample
{
    public class WaveformService
    {
        private readonly Dictionary<string, IWaveformGenerator> _generators;

        public WaveformService(Dictionary<string, IWaveformGenerator> generators)
        {
            _generators = generators;
        }

        public double[] GenerateWaveform(List<WaveformGridRow> gridData, int sampleRate)
        {
            var finalWaveform = new List<double>();

            foreach (var row in gridData)
            {
                if (!_generators.ContainsKey(row.WaveformType))
                    throw new ArgumentException($"Unsupported waveform type: {row.WaveformType}");

                var generator = _generators[row.WaveformType];
                int samples = (int)(row.Duration * sampleRate);
                double frequencyStep = row.FrequencySweep ? (row.EndFrequency - row.StartFrequency) / samples : 0;
                double amplitudeStep = row.AmplitudeSweep ? (row.EndAmplitude - row.StartAmplitude) / samples : 0;
                double offsetStep = row.DCOffsetSweep ? (row.EndDCOffset - row.StartDCOffset) / samples : 0;

                for (int i = 0; i < samples; i++)
                {
                    double currentFrequency = row.StartFrequency + frequencyStep * i;
                    double currentAmplitude = row.StartAmplitude + amplitudeStep * i;
                    double currentOffset = row.StartDCOffset + offsetStep * i;

                    double[] waveform = generator.Generate(1, currentFrequency, currentAmplitude);
                    finalWaveform.Add(waveform[0] + currentOffset);
                }
            }

            return finalWaveform.ToArray();
        }
    }
}
