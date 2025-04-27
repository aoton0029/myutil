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
    public partial class UcItemFilterCheckBoxPanel: UserControl
    {
        private List<CheckBox> _checkBoxes = new List<CheckBox>();
        public event EventHandler FilterChanged;

        public UcItemFilterCheckBoxPanel()
        {
            InitializeComponent();
        }

        public void SetFilterOptions(IEnumerable<string> options)
        {
            flowLayoutPanel1.Controls.Clear();
            _checkBoxes.Clear();

            foreach (var option in options)
            {
                var checkBox = CreateButtonStyleCheckBox(option);
                _checkBoxes.Add(checkBox);
                flowLayoutPanel1.Controls.Add(checkBox);
            }
        }

        private CheckBox CreateButtonStyleCheckBox(string text)
        {
            var checkBox = new CheckBox
            {
                Appearance = Appearance.Button,
                Text = text,
                AutoSize = true,
                Margin = new Padding(5),
                TextAlign = ContentAlignment.MiddleCenter
            };

            checkBox.CheckedChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);

            return checkBox;
        }

        public List<string> GetSelectedOptions()
        {
            var selected = new List<string>();
            foreach (var cb in _checkBoxes)
            {
                if (cb.Checked)
                {
                    selected.Add(cb.Text);
                }
            }
            return selected;
        }
    }
}
