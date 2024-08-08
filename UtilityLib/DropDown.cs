using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    internal class DropDown
    {
        private ToolStripDropDown dropDown;
        private Control embeddedControl;

        public DropDown(Control control)
        {
            embeddedControl = control;
            InitializeDropDown();
        }

        private void InitializeDropDown()
        {
            if (embeddedControl is Form) 
            {
                (embeddedControl as Form).TopLevel = false;
                (embeddedControl as Form).FormBorderStyle = FormBorderStyle.None;
            }
            embeddedControl.AutoSize = false;
            embeddedControl.Dock = DockStyle.None;
 
            ToolStripControlHost host = new ToolStripControlHost(embeddedControl)
            {
                Size = embeddedControl.Size,
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
            };

            dropDown = new ToolStripDropDown
            {
                //AutoClose = false,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
            };

            dropDown.Items.Add(host);
        }

        public void Show(Control parentControl)
        {
            if (dropDown != null && !dropDown.Visible)
            {
               dropDown.Show(parentControl, 0, parentControl.Height);
            }
        }

        public void Show()
        {
            if (dropDown != null && !dropDown.Visible)
            {
                dropDown.Show(Cursor.Position);
            }
        }

        public void Close()
        {
            if (dropDown != null && dropDown.Visible)
            {
                dropDown.Close();
            }
        }
    }
}
