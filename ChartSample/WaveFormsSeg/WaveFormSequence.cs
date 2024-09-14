using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample.WaveFormsSeg
{
    public class WaveformSequence
    {
        public List<WaveformSegment> Segments { get; set; } = new List<WaveformSegment>();

        // 波形遷移シーケンスを生成する
        public List<(double Time, double Value)> GenerateWaveformSequence()
        {
            List<(double Time, double Value)> waveformSequence = new List<(double Time, double Value)>();

            // 各セグメントの波形を生成
            foreach (var segment in Segments)
            {
                double timeInterval = 0.01;  // 時間の刻み
                for (double t = segment.StartSeconds; t <= segment.EndSeconds; t += timeInterval)
                {
                    // セグメントごとに適切な波形の値を取得
                    double value = segment.GenerateWaveformValue(t);
                    waveformSequence.Add((t, value));
                }
            }

            return waveformSequence;
        }
    }
}
