using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControlSample
{
    public partial class UcDD : UserControl
    {
        public UcDD()
        {
            InitializeComponent();
        }

        private void UcDD_DragEnter(object sender, DragEventArgs e)
        {
            // ドラッグされているデータが特定の型（例: カスタムオブジェクト）か確認
            if (e.Data.GetDataPresent(typeof(MyDataObject))) // MyDataObject はデータの型
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void UcDD_DragDrop(object sender, DragEventArgs e)
        {
            // ドラッグされているデータを取得
            if (e.Data.GetDataPresent(typeof(MyDataObject)))
            {
                var droppedData = (MyDataObject)e.Data.GetData(typeof(MyDataObject));
                // データをコントロールで利用
                ProcessData(droppedData);
            }
        }

        // データを処理するメソッド
        private void ProcessData(MyDataObject data)
        {
            // ユーザーコントロール内の処理（例: ラベルにデータを表示）
            label1.Text = $"受け取ったデータ: {data.Name}";
        }
    }
}
