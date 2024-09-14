using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample.WaveFormsSeg
{
    public class WaveformSegment
    {
        public enum WaveformType
        {
            Sine,
            Square,
            Triangle,
            Sawtooth
        }

        public double Period { get; set; }  // 周期 (秒)
        public double Frequency { get; set; }  // 周波数 (Hz)
        public double Amplitude { get; set; }  // 振幅 (V)
        public double StartSeconds { get; set; }  // 開始秒数
        public double EndSeconds { get; set; }  // 終了秒数
        public WaveformType WaveType { get; set; }  // 波形の種類

        // 任意のメソッドで波形を生成
        public List<(double Time, double Value)> GenerateWaveform()
        {
            List<(double Time, double Value)> waveform = new List<(double Time, double Value)>();
            double timeInterval = 0.01;  // 時間の刻み（必要に応じて調整）
            for (double t = StartSeconds; t <= EndSeconds; t += timeInterval)
            {
                double value = GenerateWaveformValue(t);
                waveform.Add((t, value));
            }
            return waveform;
        }

        // 波形の種類に応じた値を生成する
        public double GenerateWaveformValue(double time)
        {
            double phase = 2 * Math.PI * Frequency * time;
            switch (WaveType)
            {
                case WaveformType.Sine:
                    return Amplitude * Math.Sin(phase);

                case WaveformType.Square:
                    return Amplitude * (Math.Sin(phase) >= 0 ? 1 : -1);

                case WaveformType.Triangle:
                    return Amplitude * (2 / Math.PI) * Math.Asin(Math.Sin(phase));

                case WaveformType.Sawtooth:
                    return Amplitude * (2 * (time * Frequency - Math.Floor(time * Frequency + 0.5)));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
