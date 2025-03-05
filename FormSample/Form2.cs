using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormSample
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            btnClose.Click += (s, e) => Close();
            btnMaximize.Click += (s, e) => WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;
            menuStrip1.Renderer = new CustomMenuRenderer();
        }

        // カスタムメニューレンダラー（MenuStripの背景色変更）
        public class CustomMenuRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(Brushes.LightBlue, e.Item.Bounds);
                }
                else
                {
                    e.Graphics.FillRectangle(Brushes.DodgerBlue, e.Item.Bounds); // メニュー背景色
                }
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (SolidBrush brush = new SolidBrush(Color.DodgerBlue)) // メニュー全体の背景色
                {
                    e.Graphics.FillRectangle(brush, e.AffectedBounds);
                }
            }
        }
    }
}
