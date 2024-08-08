using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    public partial class Form1 : Form
    {
        DropDown d;

        public Form1()
        {
            InitializeComponent();
            //d = new DropDown(new FormDropDown());
            d = new DropDown(new UcDropDown());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            d.Show(button1);
        }
    }
}
