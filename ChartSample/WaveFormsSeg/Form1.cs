using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static ChartSample.WaveFormsSeg.WaveformStep;

namespace ChartSample.WaveFormsSeg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeChart();
            // サンプルのWaveformStepリストを作成
            List<WaveformStep> steps = new List<WaveformStep>()
            {
                // 1秒間、周波数が10Hzから20Hzにリニアスイープする正弦波
                new WaveformStep(1.0, 10, 20, 1.0, 0, WaveformType.Sine, SweepShape.Linear),
    
                // 2秒間、周波数が5Hzから50Hzにログスイープする矩形波
                new WaveformStep(2.0, 5, 50, 0.5, 1.0, WaveformType.Square, SweepShape.Logarithmic),
            };

            // シーケンスを生成
            WaveformSequence sequence = new WaveformSequence(steps, 1000);  // 1000Hzのサンプルレート

            // 全区間のシーケンスデータを生成
            List<double> data = sequence.GenerateFullSequence();

            // 生成したデータをチャートに表示
            PlotWaveform(data, 1000);
        }

        private void InitializeChart()
        {
            chart1.Series.Clear();
            ChartArea chartArea = new ChartArea("WaveformArea");
            chart1.ChartAreas.Add(chartArea);

            Series waveformSeries = new Series("Waveform")
            {
                ChartType = SeriesChartType.Line
            };

            chart1.Series.Add(waveformSeries);
            this.Controls.Add(chart1);
        }

        // 波形データを生成してChartに表示
        private void PlotWaveform(List<double> waveformData, double sampleRate)
        {
            var series = chart1.Series["Waveform"];
            series.Points.Clear();  // 前のデータをクリア

            double time = 0;
            double timeStep = 1.0 / sampleRate;

            for (int i = 0; i < waveformData.Count; i++)
            {
                series.Points.AddXY(time, waveformData[i]);
                time += timeStep;
            }
        }
    }
}
