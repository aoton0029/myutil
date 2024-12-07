using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.UserContrtols
{
    public partial class UcToastNotification : UserControl
    {
        public UcToastNotification()
        {
            InitializeComponent();

            timer1.Interval = 3000;
            timer1.Tick += (s, e) => this.Hide();
        }

        public void ShowNotification(Form parentForm, string message)
        {
            label1.Text = message;

            // フォームの右下位置を計算
            var x = parentForm.ClientSize.Width - this.Width - 10;
            var y = parentForm.ClientSize.Height - this.Height - 10;

            this.Location = new Point(x, y);
            parentForm.Controls.Add(this);
            this.BringToFront();
            this.Show();

            // タイマー開始
            timer1.Start();
        }
    }
}
