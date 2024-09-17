using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample.WaveFormsSeg
{
    public class FadeModifier : IWaveformModifier
    {
        private bool fadeIn;
        private bool fadeOut;

        public FadeModifier(bool fadeIn, bool fadeOut)
        {
            this.fadeIn = fadeIn;
            this.fadeOut = fadeOut;
        }

        public double Modify(double time, double baseAmplitude, WaveformStep step)
        {
            double fadeFactor = 1.0;

            if (fadeIn)
            {
                fadeFactor *= (time - step.StartTime) / step.Period;
            }

            if (fadeOut)
            {
                fadeFactor *= (step.EndTime - time) / step.Period;
            }

            return baseAmplitude * fadeFactor;
        }
    }

    public class BurstModifier : IWaveformModifier
    {
        private double burstDuration;

        public BurstModifier(double burstDuration)
        {
            this.burstDuration = burstDuration;
        }

        public double Modify(double time, double baseAmplitude, WaveformStep step)
        {
            double burstFactor = (time - step.StartTime) % burstDuration < burstDuration / 2 ? 1 : 0;
            return baseAmplitude * burstFactor;
        }
    }

    public class SweepModifier : IWaveformModifier
    {
        private double startFrequency;
        private double endFrequency;

        public SweepModifier(double startFrequency, double endFrequency)
        {
            this.startFrequency = startFrequency;
            this.endFrequency = endFrequency;
        }

        public double Modify(double time, double baseAmplitude, WaveformStep step)
        {
            double sweepFrequency = startFrequency + ((endFrequency - startFrequency) * (time - step.StartTime) / step.Period);
            return step.Amplitude * Math.Sin(2 * Math.PI * sweepFrequency * (time - step.StartTime));
        }
    }

}
