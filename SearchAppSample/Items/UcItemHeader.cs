using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SearchAppSample.Items
{
    public partial class UcItemHeader: UserControl
    {
        public string Title { get => lblTitle.Text; set => lblTitle.Text = value; }

        public UcItemHeader()
        {
            InitializeComponent();
        }
    }
}
