using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Forms
{
    public class ToastForm : Form
    {
        private System.Timers.Timer timer;

        public ToastForm(string message, int duration = 3000)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.Black;
            this.Opacity = 0.8;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.AutoSize = true;
            this.Padding = new Padding(10);

            Label label = new Label();
            label.ForeColor = Color.White;
            label.Text = message;
            label.AutoSize = true;

            this.Controls.Add(label);

            // タイマーを設定して自動的に閉じるようにする
            timer = new System.Timers.Timer();
            timer.Interval = duration;
            timer.Tick += (s, e) => { this.Close(); };
            timer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // トーストフォームの表示位置を設定する
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - this.Width - 10, workingArea.Bottom - this.Height - 10);
        }
    }
}
