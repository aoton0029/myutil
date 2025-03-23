using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PageNavigationSample.Test1;

namespace PageNavigationSample
{
    public partial class UcPage2: UserControl
    {
        DataMediator _dataMediator;
        NavigationService _nav;

        public UcPage2(NavigationService nav, DataMediator dataMediator)
        {
            InitializeComponent();
            _nav = nav;
            _dataMediator = dataMediator;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _nav.Navigate<UcPage1>();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            _nav.Navigate<UcPage3>();
        }
    }
}
