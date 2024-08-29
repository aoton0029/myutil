using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample.Sample2
{
    public partial class UcHome : UserControl
    {
        private MainController _controller;

        public UcHome(MainController controller)
        {
            InitializeComponent();
            _controller = controller;
            
        }
    }
}
