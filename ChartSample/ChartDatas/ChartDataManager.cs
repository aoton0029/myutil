using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample
{
    public class ChartDataManager
    {
        public List<ChartData> DataList { get; set; } = new List<ChartData>();

        // チャートデータを追加
        public void AddData(ChartData data)
        {
            DataList.Add(data);
        }

        // 指定した時刻における振幅のリストを取得
        public List<double> GetAmplitudes(double time)
        {
            var amplitudes = new List<double>();
            foreach (var data in DataList)
            {
                if (time >= data.StartTime && time <= data.EndTime)
                {
                    amplitudes.Add(data.CalculateAmplitude(time));
                }
            }
            return amplitudes;
        }

        // 指定した範囲の時刻に対する全てのデータポイントを取得
        public List<(double Time, double Amplitude)> GetPlotData(double startTime, double endTime, double step)
        {
            var plotData = new List<(double Time, double Amplitude)>();
            for (double time = startTime; time <= endTime; time += step)
            {
                var amplitudes = GetAmplitudes(time);
                foreach (var amplitude in amplitudes)
                {
                    plotData.Add((time, amplitude));
                }
            }
            return plotData;
        }
    }
}
