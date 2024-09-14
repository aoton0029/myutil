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
using static ChartSample.WaveFormsSeg.WaveformSegment;

namespace ChartSample.WaveFormsSeg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            DisplayWaveform(CreateSampleData());
        }

        private WaveformSequence CreateSampleData()
        {
            var sequence = new WaveformSequence();

            // 正弦波セグメント
            WaveformSegment sineSegment = new WaveformSegment
            {
                Period = 1.0,
                Frequency = 1.0,
                Amplitude = 5.0,
                StartSeconds = 0,
                EndSeconds = 5,
                WaveType = WaveformType.Sine
            };
            sequence.Segments.Add( sineSegment );

            // 矩形波セグメント
            WaveformSegment squareSegment = new WaveformSegment
            {
                Period = 1.0,
                Frequency = 1.0,
                Amplitude = 5.0,
                StartSeconds = 5,
                EndSeconds = 10,
                WaveType = WaveformType.Square
            };
            sequence.Segments.Add(squareSegment);

            // 三角波セグメント
            WaveformSegment triangleSegment = new WaveformSegment
            {
                Period = 1.0,
                Frequency = 1.0,
                Amplitude = 5.0,
                StartSeconds = 10,
                EndSeconds = 15,
                WaveType = WaveformType.Triangle
            };
            sequence.Segments.Add(triangleSegment);

            // 方形波セグメント
            WaveformSegment sawtoothSegment = new WaveformSegment
            {
                Period = 1.0,
                Frequency = 1.0,
                Amplitude = 5.0,
                StartSeconds = 15,
                EndSeconds = 20,
                WaveType = WaveformType.Sawtooth
            };
            sequence.Segments.Add(sawtoothSegment);

            return sequence;
        }

        private void DisplayWaveform(WaveformSequence sequence)
        {
            var chart = chart1;
            var chartArea = new ChartArea();

            var series = new Series
            {
                ChartType = SeriesChartType.Line
            };

            // 波形データの追加と最小値、最大値の計算
            var waveformData = sequence.GenerateWaveformSequence();
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            foreach (var point in waveformData)
            {
                series.Points.AddXY(point.Time, point.Value);
                if (point.Value < minValue) minValue = point.Value;
                if (point.Value > maxValue) maxValue = point.Value;
            }

            // Y軸の最小値、最大値の1.1倍を設定
            double rangePadding = 0.1 * Math.Max(Math.Abs(maxValue), Math.Abs(minValue));
            chartArea.AxisY.Minimum = minValue - rangePadding;
            chartArea.AxisY.Maximum = maxValue + rangePadding;

            // X軸、Y軸の自動調整を有効化
            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

            // Y軸の0を常に表示
            chartArea.AxisY.Crossing = 0;

            chart.ChartAreas.Add(chartArea);
            chart.Series.Add(series);
            chart.Dock = DockStyle.Fill;

            // フォームのPanelにチャートを追加
            this.Controls.Add(chart);
        }
    }
}
