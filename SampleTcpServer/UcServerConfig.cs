using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleTcpServer
{
    public partial class UcServerConfig : UserControl
    {
        public bool IsUse { get => chkUse.Checked; set => chkUse.Checked = value; }
        public int IP1 { get => (int)nud1.Value; set => nud1.Value = value; }
        public int IP2 { get => (int)nud2.Value; set => nud2.Value = value; }
        public int IP3 { get => (int)nud3.Value; set => nud3.Value = value; }
        public int IP4 { get => (int)nud4.Value; set => nud4.Value = value; }
        public IPAddress IPAddress => IPAddress.Parse(string.Join(".", IP1, IP2, IP3, IP4));

        public UcServerConfig()
        {
            InitializeComponent();
        }
    }
}
