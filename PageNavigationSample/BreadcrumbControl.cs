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
    public partial class BreadcrumbControl : UserControl
    {
        private List<string> items = new List<string>();
        public event EventHandler<BreadcrumbClickEventArgs> ItemClicked;

        public string Separator { get; set; } = " > ";

        public BreadcrumbControl()
        {
            InitializeComponent();
        }

        public void SetItems(List<string> paths)
        {
            items = paths;
            UpdateBreadcrumb();
        }

        private void UpdateBreadcrumb()
        {
            flowPanel.Controls.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                var linkLabel = new LinkLabel
                {
                    Text = items[i],
                    AutoSize = true,
                    Tag = i,
                    LinkColor = Color.Blue
                };

                linkLabel.Click += LinkLabel_Click;
                flowPanel.Controls.Add(linkLabel);

                if (i < items.Count - 1)
                {
                    var separatorLabel = new Label
                    {
                        Text = Separator,
                        AutoSize = true
                    };
                    flowPanel.Controls.Add(separatorLabel);
                }
            }
        }

        private void LinkLabel_Click(object sender, EventArgs e)
        {
            if (sender is LinkLabel linkLabel && linkLabel.Tag is int index)
            {
                ItemClicked?.Invoke(this, new BreadcrumbClickEventArgs(index, items[index]));
            }
        }
    }

    //public class BreadcrumbClickEventArgs : EventArgs
    //{
    //    public int Index { get; }
    //    public string Item { get; }

    //    public BreadcrumbClickEventArgs(int index, string item)
    //    {
    //        Index = index;
    //        Item = item;
    //    }
    //}
}
