using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib
{
    public class DualListBox
    {
        private ListBox listBox1;
        private ListBox listBox2;
        public ListBox.ObjectCollection ItemsInListBox1 => listBox1.Items;
        public ListBox.ObjectCollection ItemsInListBox2 => listBox2.Items;

        public DualListBox(ListBox lb1, ListBox lb2, Button btnToRight, Button btnToLeft)
        {
            listBox1 = lb1;
            listBox2 = lb2;
            btnToLeft.Click += MoveToLeftButton_Click;
            btnToRight.Click += MoveToRightButton_Click;
        }

        public void SetItems(ListBox listBox, object[] items)
        {
            listBox.Items.Clear();
            listBox.Items.AddRange(items);
        }

        private void MoveToRightButton_Click(object sender, EventArgs e)
        {
            MoveItems(listBox1, listBox2);
        }

        private void MoveToLeftButton_Click(object sender, EventArgs e)
        {
            MoveItems(listBox2, listBox1);
        }

        private void MoveItems(ListBox source, ListBox destination)
        {
            var selectedItems = new object[source.SelectedItems.Count];
            source.SelectedItems.CopyTo(selectedItems, 0);

            foreach (var item in selectedItems)
            {
                destination.Items.Add(item);
                source.Items.Remove(item);
            }
        }
    }
}
