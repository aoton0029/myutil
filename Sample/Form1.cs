using Sample.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            NavigationService.Instance.Initialize(panel2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigateTo(new UcHome());
        }

        private void btnSeihin_Click(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigateTo(new UcDashboard());
        }

        private void btnParts_Click(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigateTo(new UcSeihin());
        }

        private void btnOption_Click(object sender, EventArgs e)
        {
            NavigationService.Instance.NavigateTo(new UcOptions());
        }
    }
}
