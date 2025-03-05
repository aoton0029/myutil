using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormSample
{
    public partial class UcTitleBar : UserControl
    {
        private Form parentForm;
        private Color? _foreColor;

        public Color? ForeColor
        {
            get => _foreColor;
            set => _foreColor = value ?? Color.Black;
        } 

        public Color TitleBarColor
        {
            get => this.BackColor;
            set
            {
                this.BackColor = value;
                menuStrip1.BackColor = value;
                btnClose.BackColor = value;
                btnMaximize.BackColor = value;
                btnMinimize.BackColor = value;
                
            }
        }

        public UcTitleBar()
        {
            InitializeComponent();
            btnClose.Click += (s, e) => parentForm.Close();
            btnMaximize.Click += (s, e) => parentForm.WindowState = parentForm.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            btnMinimize.Click += (s, e) => parentForm.WindowState = FormWindowState.Minimized;
        }


        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (this.Parent is Form form)
            {
                parentForm = form;
                parentForm.FormBorderStyle = FormBorderStyle.None; // デフォルトのタイトルバーを削除
                if (parentForm.Icon != null)
                {
                    picAppIcon.Image = parentForm.Icon.ToBitmap(); // 親フォームのアイコンを設定
                }
            }
        }
    }
}
