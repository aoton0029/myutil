using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartSample
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms.DataVisualization.Charting;

    public class ChartSettings
    {
        public Chart ChartControl { get; set; }
        public List<SeriesData> SeriesList { get; private set; }
        public string Title { get; set; }
        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }

        public ChartSettings(Chart chart)
        {
            ChartControl = chart;
            SeriesList = new List<SeriesData>();
        }

        public void AddSeries(string seriesName, SeriesChartType chartType)
        {
            var newSeries = new SeriesData
            {
                SeriesName = seriesName,
                ChartType = chartType
            };
            SeriesList.Add(newSeries);
        }

        public void AddDataPoint(string seriesName, double x, double y)
        {
            var seriesData = SeriesList.Find(s => s.SeriesName == seriesName);
            if (seriesData != null)
            {
                seriesData.XValues.Add(x);
                seriesData.YValues.Add(y);
            }
            else
            {
                throw new ArgumentException($"Series '{seriesName}' does not exist.");
            }
        }

        public void UpdateChart()
        {
            ChartControl.Series.Clear();

            foreach (var seriesData in SeriesList)
            {
                var series = new Series(seriesData.SeriesName)
                {
                    ChartType = seriesData.ChartType
                };

                for (int i = 0; i < seriesData.XValues.Count; i++)
                {
                    series.Points.AddXY(seriesData.XValues[i], seriesData.YValues[i]);
                }

                ChartControl.Series.Add(series);
            }

            ChartControl.Titles.Clear();
            if (!string.IsNullOrEmpty(Title))
            {
                ChartControl.Titles.Add(Title);
            }

            ChartControl.ChartAreas[0].AxisX.Title = XAxisTitle;
            ChartControl.ChartAreas[0].AxisY.Title = YAxisTitle;

            // 軸の最小値と最大値の設定
            ChartControl.ChartAreas[0].AxisX.Minimum = XMin;
            ChartControl.ChartAreas[0].AxisX.Maximum = XMax;
            ChartControl.ChartAreas[0].AxisY.Minimum = YMin;
            ChartControl.ChartAreas[0].AxisY.Maximum = YMax;

            ChartControl.Invalidate(); // 再描画
        }

        public void ClearData()
        {
            foreach (var seriesData in SeriesList)
            {
                seriesData.XValues.Clear();
                seriesData.YValues.Clear();
            }
            ChartControl.Series.Clear();
        }

        public class SeriesData
        {
            public string SeriesName { get; set; }
            public List<double> XValues { get; private set; }
            public List<double> YValues { get; private set; }
            public SeriesChartType ChartType { get; set; }

            public SeriesData()
            {
                XValues = new List<double>();
                YValues = new List<double>();
            }
        }
    }
}
