using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.Pagenations
{
    public partial class Form1 : Form
    {
        private List<Control> _allItems;
        private List<Control> _currentPageItems;

        public Form1()
        {
            InitializeComponent();
            //paginationControl1.PageChanged += PaginationControl1_PageChanged;
            //LoadData();
        }

        //private void LoadData()
        //{
        //    _allItems = GetDataFromSource();
        //    paginationControl1.SetTotalItems(_allItems.Count);
        //    UpdateDataGrid();
        //}

        //private void PaginationControl1_PageChanged(object sender, EventArgs e)
        //{
        //    UpdateDataGrid();
        //}

        //private void UpdateDataGrid()
        //{
        //    int startIndex = (paginationControl1.CurrentPage - 1) * paginationControl1.ItemsPerPage;
        //    _currentPageItems = _allItems.Skip(startIndex).Take(paginationControl1.ItemsPerPage).ToList();
        //    dataGridView1.DataSource = _currentPageItems;
        //}
    }
}
