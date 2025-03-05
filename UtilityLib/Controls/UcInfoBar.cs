using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.Controls
{
    public partial class UcInfoBar: UserControl
    {
        private System.Windows.Forms.Timer closeTimer;

        public enum InfoType
        {
            Info,
            Warning,
            Error
        }

        public UcInfoBar()
        {
            InitializeComponent();
            closeTimer = new System.Windows.Forms.Timer { Interval = 5000 }; // 5秒後に自動で閉じる
            closeTimer.Tick += (s, e) => this.Hide();

        }

        public void ShowMessage(string message, InfoType type, int duration = 5000)
        {
            messageLabel.Text = message;
            SetBackgroundColor(type);

            this.Show();
            closeTimer.Interval = duration;
            closeTimer.Start();
        }

        private void SetBackgroundColor(InfoType type)
        {
            switch (type)
            {
                case InfoType.Info:
                    this.BackColor = Color.LightBlue;
                    break;
                case InfoType.Warning:
                    this.BackColor = Color.Orange;
                    break;
                case InfoType.Error:
                    this.BackColor = Color.Red;
                    messageLabel.ForeColor = Color.White;
                    break;
            }
        }
    }
}
