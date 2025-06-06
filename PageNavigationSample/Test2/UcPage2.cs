﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample.Test2
{
    public partial class UcPage2 : UcPageBase
    {
        public UcPage2(ServiceProvider provider) : base(provider)
        {
            InitializeComponent();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            _nav.GoPrev();

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _nav.GoNext<UcPage3>();

        }

        public override void OnShown(NavigationContext context)
        {
            Debug.Print(context.CurrentPage.Name);

        }
    }
}
