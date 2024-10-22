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

namespace ChartSample
{
    public partial class Form1 : Form
    {
        private DataPoint _selectedPoint = null;  // ドラッグ中の点
        private int _selectedIndex = -1;          // 選択された点のインデックス

        public Form1()
        {
            InitializeComponent();
            InitializeChart();
        }

        private void InitializeChart()
        {
            // Chartの初期設定
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add(new ChartArea("MainArea"));

            // 折れ線グラフのSeries設定
            Series lineSeries = new Series("LineSeries");
            lineSeries.ChartType = SeriesChartType.Line;
            lineSeries.XValueType = ChartValueType.Double;
            lineSeries.YValueType = ChartValueType.Double;
            chart1.Series.Add(lineSeries);

            // マウスイベントを追加
            chart1.MouseClick += Chart1_MouseClick;
            chart1.MouseDown += Chart1_MouseDown;
            chart1.MouseMove += Chart1_MouseMove;
            chart1.MouseUp += Chart1_MouseUp;
        }

        private void Chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (_selectedPoint == null)
            {
                // マウスクリック位置を取得
                var chartArea = chart1.ChartAreas[0];
                var xValue = chartArea.AxisX.PixelPositionToValue(e.X);
                var yValue = chartArea.AxisY.PixelPositionToValue(e.Y);

                // クリックした位置に点を追加
                chart1.Series["LineSeries"].Points.AddXY(xValue, yValue);
            }
        }

        private void Chart1_MouseDown(object sender, MouseEventArgs e)
        {
            // マウスがクリックされたとき、クリック位置に最も近い点を探す
            var hit = chart1.HitTest(e.X, e.Y);
            if (hit.ChartElementType == ChartElementType.DataPoint)
            {
                _selectedIndex = hit.PointIndex;
                _selectedPoint = chart1.Series["LineSeries"].Points[_selectedIndex];
            }
        }

        private void Chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_selectedPoint != null && _selectedIndex >= 0)
            {
                // マウスがドラッグされた場合、Y座標を更新する（上下に動かす）
                var chartArea = chart1.ChartAreas[0];
                var yValue = chartArea.AxisY.PixelPositionToValue(e.Y);

                // 点のY値のみ変更（X値は固定）
                _selectedPoint.YValues[0] = yValue;
            }
        }

        private void Chart1_MouseUp(object sender, MouseEventArgs e)
        {
            // ドラッグ終了
            _selectedPoint = null;
            _selectedIndex = -1;
        }
    }
}

