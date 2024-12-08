using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.Navigations
{
    public partial class UcHome : UserControl, IUserControl
    {
        public UcHome()
        {
            InitializeComponent();
        }

        public void OnNavigated(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
