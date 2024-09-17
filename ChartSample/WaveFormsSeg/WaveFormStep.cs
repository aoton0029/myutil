using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample.WaveFormsSeg
{
    public interface IWaveformModifier
    {
        double Modify(double time, double baseAmplitude, WaveformStep step);
    }

    public class WaveformStep
    {
        public double Period { get; set; }
        public double StartFrequency { get; set; }
        public double EndFrequency { get; set; }
        public double Amplitude { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public WaveformType Type { get; set; }
        public SweepShape Sweep { get; set; }  // スイープの種類（リニア、ログ、なし）

        private List<IWaveformModifier> modifiers;

        public WaveformStep(double period, double startFrequency, double endFrequency, double amplitude, double startTime, WaveformType type, SweepShape sweep = SweepShape.None)
        {
            Period = period;
            StartFrequency = startFrequency;
            EndFrequency = endFrequency;
            Amplitude = amplitude;
            StartTime = startTime;
            EndTime = StartTime + Period;
            Type = type;
            Sweep = sweep;
            modifiers = new List<IWaveformModifier>();
        }

        // 依存性注入でモディファイアを追加
        public void AddModifier(IWaveformModifier modifier)
        {
            modifiers.Add(modifier);
        }

        // 現在の時間に基づいて周波数を計算する
        private double GetCurrentFrequency(double time)
        {
            double t = (time - StartTime) / Period;  // 正規化された時間 (0から1)
            switch (Sweep)
            {
                case SweepShape.Linear:
                    return StartFrequency + (EndFrequency - StartFrequency) * t;
                case SweepShape.Logarithmic:
                    // 対数スイープ (0除算を避けるために微小値を足す)
                    return StartFrequency * Math.Pow(EndFrequency / StartFrequency, t);
                default:
                    return StartFrequency;
            }
        }

        // ベースの波形生成
        public double GenerateWaveformValue(double time)
        {
            double frequency = GetCurrentFrequency(time);  // 現在の周波数を取得
            double rad = 2 * Math.PI * frequency * (time - StartTime);

            switch (Type)
            {
                case WaveformType.Sine:
                    return Amplitude * Math.Sin(rad);
                case WaveformType.Square:
                    return Amplitude * (Math.Sin(rad) >= 0 ? 1 : -1);
                case WaveformType.Sawtooth:
                    return Amplitude * (2 * (time * frequency - Math.Floor(time * frequency + 0.5)));
                case WaveformType.Triangle:
                    return 2 * Amplitude * Math.Abs(2 * (time * frequency - Math.Floor(time * frequency + 0.5))) - Amplitude;
                default:
                    return 0;
            }
        }

        // モディファイアリストを取得する
        public List<IWaveformModifier> GetModifiers()
        {
            return modifiers;
        }
    }

}
