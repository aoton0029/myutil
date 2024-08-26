using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample.Sample1
{
    public partial class UserControl1 : BasePage
    {
        public UserControl1(MainController controller) : base(controller)
        {
            InitializeComponent();
        }

        public override void IShown()
        {
            
        }

        public override void UpdateData(object data)
        {
            
        }
    }
}
