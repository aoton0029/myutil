using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample.Test2
{
    public partial class UcPageBase : UserControl, IShown, INextPageDecider
    {
        protected NavigationFlowService _nav => _provider.Resolve<NavigationFlowService>();
        protected readonly ServiceProvider _provider;

        private UcPageBase()
        {
            InitializeComponent();
        }

        public UcPageBase(ServiceProvider provider)
        {
            InitializeComponent();
            _provider = provider;
        }

        public virtual Type? DecideNextPage(NavigationContext context)
        {
            return context.DefaultNextPage;
        }

        public virtual void OnShown(NavigationContext context)
        {

        }
    }
}
