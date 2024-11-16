using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample.Items
{
    public partial class ItemNumber : UserControl
    {
        // プロパティ: 数量
        public int Number
        {
            get => int.TryParse(txtNumber.Text, out int result) ? result : 0;
            set => txtNumber.Text = value.ToString();
        }

        public ItemNumber()
        {
            InitializeComponent();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            Number = Math.Max(0, Number - 1);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            Number++;
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtNumber.Text, out _))
            {
                txtNumber.Text = "0"; // 無効な入力はリセット
            }
        }
    }
}
