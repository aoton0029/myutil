using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControlSample
{
    public partial class UcMenubar : UserControl
    {
        public UcMenubar()
        {
            InitializeComponent();
        }

        // メニュー項目を外部から設定できるようにプロパティを作成
        //public List<ToolStripMenuItem> MenuItems
        //{
        //    get => new List<ToolStripMenuItem>(menuStrip1.Items.Cast<ToolStripMenuItem>());
        //    set
        //    {
        //        menuStrip1.Items.Clear();
        //        menuStrip1.Items.AddRange(value.ToArray());
        //    }
        //}

        //public void AddMenuItem(string text, EventHandler onClick)
        //{
        //    var menuItem = new ToolStripMenuItem(text);
        //    menuItem.Click += onClick;
        //    menuStrip1.Items.Add(menuItem);
        //}


    }
}
