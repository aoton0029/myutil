using ChartSample.Models;
using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ChartSample
{
    public partial class Form2 : Form
    {
        BindingSource bindingSource;
        WaveFormMng _mng = new WaveFormMng();
        List<double> points;

        public Form2()
        {
            InitializeComponent();

            DataGridViewComboBoxColumn comboBoxColumn = (DataGridViewComboBoxColumn)grid.Columns[0];
            comboBoxColumn.SetEnumDataSource<Common.WaveformType>();

            bindingSource = new BindingSource();
            bindingSource.DataSource = _mng.datas;
            grid.DataSource = bindingSource;
            bindingSource.ListChanged += BindingSource_ListChanged;

            

        }

        private void BindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                // 新しく追加されたMyDataオブジェクトを取得
                DataGridItem newItem = bindingSource[e.NewIndex] as DataGridItem;
                if (newItem != null)
                {
                    // PropertyChangedイベントを設定
                    newItem.PropertyChanged += MyData_PropertyChanged;
                    //MessageBox.Show($"New item added: {newItem.WaveType}");
                }
            }
        }

        // MyDataのプロパティが変更されたときに呼ばれるイベントハンドラ
        private void MyData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DataGridItem changedItem = sender as DataGridItem;
            if (changedItem != null)
            {
                Debug.WriteLine($"Property {e.PropertyName} changed on item {changedItem.WaveType}");
                List<ChartItem> c =  _mng.Generate();
                cartesianChart1.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Series 1",
                        Values = new ChartValues<double>(c.Select(x => x.Value)),
                        Stroke = System.Windows.Media.Brushes.Blue,    // 線の色
                        Fill = System.Windows.Media.Brushes.Transparent,  // 塗りつぶしを透明に
                        StrokeThickness = 2,   // 線の太さ
                        PointGeometry = null,  // データポイントを非表示に
                    }
                };
            }
        }

    }

}
