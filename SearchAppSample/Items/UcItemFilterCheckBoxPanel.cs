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

        [Category("CheckBox")]
        public Color CheckBoxBackColor { get; set; } = SystemColors.Control;
        public Color CheckBoxForeColor { get; set; } = SystemColors.ControlText;
        public Color CheckBoxCheckedBackColor { get; set; } = SystemColors.Highlight;
        public Color CheckBoxCheckedForeColor { get; set; } = SystemColors.HighlightText;
        public Font CheckBoxFont { get; set; } = SystemFonts.DefaultFont;
        public FlatStyle CheckBoxFlatStyle { get; set; } = FlatStyle.Standard;
        public BorderStyle CheckBoxBorderStyle { get; set; } = BorderStyle.None;
        public Padding CheckBoxPadding { get; set; } = new Padding(5);
        public Padding CheckBoxMargin { get; set; } = new Padding(5);

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
                Margin = CheckBoxMargin,
                Padding = CheckBoxPadding,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = CheckBoxBackColor,
                ForeColor = CheckBoxForeColor,
                Font = CheckBoxFont,
                FlatStyle = CheckBoxFlatStyle
            };

            // チェック状態変更時の処理
            checkBox.CheckedChanged += (s, e) =>
            {
                FilterChanged?.Invoke(this, EventArgs.Empty);
                UpdateCheckBoxAppearance(checkBox);
            };

            return checkBox;
        }

        private void UpdateCheckBoxAppearance(CheckBox checkBox)
        {
            // チェック状態によって色を変更
            if (checkBox.Checked)
            {
                checkBox.BackColor = CheckBoxCheckedBackColor;
                checkBox.ForeColor = CheckBoxCheckedForeColor;
            }
            else
            {
                checkBox.BackColor = CheckBoxBackColor;
                checkBox.ForeColor = CheckBoxForeColor;
            }
        }

        // すべてのチェックボックスの外観を更新
        public void UpdateAllCheckBoxesAppearance()
        {
            foreach (var checkBox in _checkBoxes)
            {
                checkBox.Font = CheckBoxFont;
                checkBox.FlatStyle = CheckBoxFlatStyle;
                checkBox.Margin = CheckBoxMargin;
                checkBox.Padding = CheckBoxPadding;
                UpdateCheckBoxAppearance(checkBox);
            }
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
