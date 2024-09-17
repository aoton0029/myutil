using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartSample.WaveFormsSeg
{
    public class WaveformSequence
    {
        private List<WaveformStep> steps;
        private double sampleRate;

        public WaveformSequence(List<WaveformStep> waveformSteps, double sampleRate)
        {
            this.steps = waveformSteps;
            this.sampleRate = sampleRate;
        }

        // 全区間の波形データを生成する
        public List<double> GenerateFullSequence()
        {
            // 全てのステップの中で最も遅い EndTime を探す
            double totalTime = steps.Max(step => step.EndTime);

            // 0秒から全体の終わりの時間までの波形データを生成
            return GenerateSequence(0, totalTime);
        }

        // 指定区間の波形データを生成する
        public List<double> GenerateSequence(double startTime, double endTime)
        {
            List<double> sequenceData = new List<double>();
            double currentTime = startTime;

            // サンプリングレートに基づいて、startTime から endTime までの波形を生成
            while (currentTime <= endTime)
            {
                double value = 0;

                // 各ステップを調べて、現在の時間がそのステップの範囲内にあるか確認
                foreach (var step in steps)
                {
                    if (currentTime >= step.StartTime && currentTime <= step.EndTime)
                    {
                        value = step.GenerateWaveformValue(currentTime - step.StartTime);

                        // モディファイアがある場合は適用
                        foreach (var modifier in step.GetModifiers())
                        {
                            value = modifier.Modify(value, currentTime - step.StartTime, step);
                        }

                        break;  // 対応するステップが見つかったのでループを抜ける
                    }
                }

                sequenceData.Add(value);
                currentTime += 1.0 / sampleRate;  // サンプリングレートに基づいて次の時間へ
            }

            return sequenceData;
        }
    }
}
