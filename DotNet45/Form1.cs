using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet45
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //DataTable table = new DataTable();

            //try
            //{
            //    table.BeginLoadData();

            //    for (int i = 0; i < 100000; i++)
            //    {
            //        object[] row = { i, $"Name_{i}", 20 + (i % 50) };
            //        table.LoadDataRow(row, true);
            //    }
            //}
            //finally
            //{
            //    table.EndLoadData();
            //}
        }
    }
}
