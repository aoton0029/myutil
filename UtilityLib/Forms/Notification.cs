using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.Forms
{

    public partial class Notification : Form
    {
        private System.Windows.Forms.Timer _timer;
        private int _displayTime; // 表示時間（ミリ秒）
        private int _fadeStep = 5; // フェードステップの量
        private bool _isClosing = false;

        public Notification(string message, int displayTime = 3000)
        {
            _displayTime = displayTime;

            // ウィンドウ設定
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Width = 300;
            Height = 80;
            BackColor = Color.FromArgb(41, 128, 185);
            Opacity = 0; // 初期透明度
            ShowInTaskbar = false;

            // メッセージラベル
            Label lblMessage = new Label
            {
                Text = message,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            Controls.Add(lblMessage);

            // タイマー初期化
            _timer = new System.Windows.Forms.Timer { Interval = 50 }; // 50msごとにタイマー発火
            _timer.Tick += Timer_Tick;

            // 表示位置
            int x = Screen.PrimaryScreen.WorkingArea.Width - Width - 10;
            int y = Screen.PrimaryScreen.WorkingArea.Height - Height - 10;
            Location = new Point(x, y);
        }

        public void ShowNotification()
        {
            Show();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isClosing)
            {
                if (Opacity < 1.0)
                {
                    Opacity += 0.1; // フェードイン
                }
                else
                {
                    _displayTime -= _timer.Interval;
                    if (_displayTime <= 0)
                    {
                        _isClosing = true;
                    }
                }
            }
            else
            {
                if (Opacity > 0)
                {
                    Opacity -= 0.1; // フェードアウト
                }
                else
                {
                    _timer.Stop();
                    Close();
                }
            }
        }
    }

}
