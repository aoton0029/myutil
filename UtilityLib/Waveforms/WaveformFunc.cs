using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Waveforms
{
    public enum Waveform
    {
        Sine,
        Square,
        Triangle,
        Sawtooth
    }

    public static class WaveformFunctions
    {
        public delegate double WaveformFunction(double time);

        public static double Sine(double time) => Math.Sin(time);

        public static double Square(double time) =>
            Math.Sign(Math.Sign(time) * (Math.PI - Math.Abs(time % (2 * Math.PI))));

        public static double Triangle(double time)
        {
            var shifted = Math.Abs((time - (Math.PI / 2)) % (2 * Math.PI)) - Math.PI;
            return (Math.Sign(shifted) * shifted / Math.PI * 2) - 1;
        }

        public static double Sawtooth(double time) =>
            ((((time + Math.PI) % (2 * Math.PI)) + (2 * Math.PI)) % (2 * Math.PI) - Math.PI) / Math.PI;

        public static WaveformFunction Create(Waveform waveform) => waveform switch
        {
            Waveform.Sine => (WaveformFunction)WaveformFunctions.Sine,
            Waveform.Square => (WaveformFunction)WaveformFunctions.Square,
            Waveform.Triangle => (WaveformFunction)WaveformFunctions.Triangle,
            Waveform.Sawtooth => (WaveformFunction)WaveformFunctions.Sawtooth,
            _ => throw new ArgumentOutOfRangeException(nameof(waveform))
        };

        public static WaveformFunction Create(WaveformParameters parameters)
        {
            var function = WaveformFunctions.Create(parameters.Waveform);
            return time => parameters.Amplitude *
                function((time * (2 * Math.PI) * parameters.Frequency) + parameters.Phase);
        }
    }

    [Serializable]
    public readonly struct WaveformParameters : IEquatable<WaveformParameters>
    {
        public WaveformParameters(Waveform waveform) : this(waveform, amplitude: 1.0)
        {
        }

        public WaveformParameters(Waveform waveform, double amplitude = 1.0, double frequency = 1 / (2 * Math.PI), double phase = 0.0)
        {
            if (waveform < Waveform.Sine || waveform > Waveform.Sawtooth)
            {
                throw new ArgumentOutOfRangeException(nameof(waveform));
            }
            if (frequency < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency));
            }
            if (amplitude < 0 || amplitude > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amplitude));
            }

            this.Waveform = waveform;
            this.Frequency = frequency;
            this.Amplitude = amplitude;
            this.Phase = phase;
        }

        public Waveform Waveform { get; }

        public double Amplitude { get; }

        public double Frequency { get; }

        public double Phase { get; }

        //public bool Equals(WaveformParameters other) => this.BinaryEquals(other);

        public override bool Equals(object? obj) => (obj is WaveformParameters other) && this.Equals(other);

        bool IEquatable<WaveformParameters>.Equals(WaveformParameters other)
        {
            throw new NotImplementedException();
        }

        //public override int GetHashCode() => this.GetBinaryHashCode();

        public static bool operator ==(WaveformParameters left, WaveformParameters right) => left.Equals(right);

        public static bool operator !=(WaveformParameters left, WaveformParameters right) => !left.Equals(right);
    }
}
