using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace PageNavigationSample.Test2
{
    public partial class UcStart : UcPageBase
    {
        public UcStart(ServiceProvider provider) : base(provider)
        {
            InitializeComponent();
        }

        public override void OnShown(NavigationContext context)
        {
            Debug.Print(context.CurrentPage.Name);
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            _nav.GoBack();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _nav.Cancel();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _nav.GoNext<UcPage1>();
        }
    }
}
