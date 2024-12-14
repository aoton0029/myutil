using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample.UserControls
{
    public partial class UcHome : UserControl
    {
        public UcHome()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigateTo(new UcDashboard());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigateTo(new UcSeihin());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //NavigationService.Instance.NavigateTo(new UcParts());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigateTo(new UcOptions());
        }
    }
}
