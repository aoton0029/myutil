using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample
{
    public partial class UcFlowPageBase : UserControl, IPage
    {
        private readonly NavigationFlowService _navigationService;

        public UcFlowPageBase(NavigationFlowService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;
        }

        public virtual void OnPageLeave(NavigationContext context)
        {

        }

        public virtual void OnPageShown(NavigationContext context)
        {

        }
    }
}
