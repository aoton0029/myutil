using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample
{
    public class ChartData
    {
        public double Period { get; set; }  // 周期 (秒)
        public double Frequency { get; set; } // 周波数 (Hz)
        public double Amplitude { get; set; } // 振幅 (V)
        public double StartTime { get; set; } // 開始秒数
        public double EndTime { get; set; }   // 終了秒数

        // コンストラクタ
        public ChartData(double period, double frequency, double amplitude, double startTime, double endTime)
        {
            Period = period;
            Frequency = frequency;
            Amplitude = amplitude;
            StartTime = startTime;
            EndTime = endTime;
        }

        // 周期に基づいて振幅を計算する関数
        public double CalculateAmplitude(double time)
        {
            // 正弦波の例: A * sin(2 * π * f * t)
            return Amplitude * Math.Sin(2 * Math.PI * Frequency * time);
        }
    }
}
