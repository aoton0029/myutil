using ChartSample.WaveFormsSeg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartSample
{
    public partial class Form2 : Form
    {
        BindingSource bindingSource;
        BindingList<DataGridItem> items;

        public Form2()
        {
            InitializeComponent();

            DataGridViewComboBoxColumn comboBoxColumn = (DataGridViewComboBoxColumn)grid.Columns[0];
            comboBoxColumn.SetEnumDataSource<WaveformType>();

            bindingSource = new BindingSource();
            items = new BindingList<DataGridItem>();
            bindingSource.DataSource = items;
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
                //MessageBox.Show($"Property {e.PropertyName} changed on item {changedItem.WaveType}");
            }
        }

    }

}
