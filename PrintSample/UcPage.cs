using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintSample
{
    public partial class UcPage : UserControl
    {
        public UcPage()
        {
            InitializeComponent();
        }

        public virtual void OnNavigatedTo(object parameter)
        {
            // ページが表示される際にパラメータを受け取る処理を実装
        }

        public virtual void OnNavigatedFrom()
        {
            // ページが離れる際に必要な処理を実装
        }
    }
}
