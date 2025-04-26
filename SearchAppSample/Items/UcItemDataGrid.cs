using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.Design;

namespace SearchAppSample.Items
{
    public partial class UcItemDataGrid: UserControl
    {
        public enum DisplayMode
        {
            Pagination,
            All
        }

            private ComboBox _pageSizeComboBox;
            private int _pageSize = 50;
            private int _currentPage = 0;
            private DataTable _sourceTable;
            private DisplayMode _displayMode = DisplayMode.Pagination;

            public Func<DataTable> FetchAllData { get; set; }
            public Func<int, int, DataTable> FetchPagedData { get; set; }
            public Func<int> FetchTotalCount { get; set; }

            public DisplayMode Mode
            {
                get => _displayMode;
                set
                {
                    _displayMode = value;
                    LoadData();
                }
            }

            public DataView CurrentView => _sourceTable?.DefaultView;

            public UcItemDataGrid()
            {
                InitializeComponent();
                InitializeControls();
            }

            private void InitializeControls()
            {
                _pageSizeComboBox = new ComboBox { Dock = DockStyle.Bottom };
                _pageSizeComboBox.Items.AddRange(new object[] { 10, 20, 50, 100 });
                _pageSizeComboBox.SelectedItem = 50;
                _pageSizeComboBox.SelectedIndexChanged += (s, e) =>
                {
                    _pageSize = (int)_pageSizeComboBox.SelectedItem;
                    _currentPage = 0;
                    LoadData();
                };

                //_nextButton.Click += (s, e) => { _currentPage++; LoadData(); };
                //_prevButton.Click += (s, e) => { if (_currentPage > 0) _currentPage--; LoadData(); };
                //_firstButton.Click += (s, e) => { _currentPage = 0; LoadData(); };
                //_lastButton.Click += (s, e) => { _currentPage = GetTotalPages() - 1; LoadData(); };
            }

            private void LoadData()
            {
                if (FetchAllData == null || FetchPagedData == null)
                    throw new InvalidOperationException("Fetch methods must be set.");

                if (Mode == DisplayMode.All)
                {
                    _sourceTable = FetchAllData();
                    _currentPage = 0;
                }
                else
                {
                    _sourceTable = FetchPagedData(_currentPage * _pageSize, _pageSize);
                }

                dataGridView1.DataSource = _sourceTable;
                UpdateNavigationButtons();
            }

            private void UpdateNavigationButtons()
            {
                bool pagination = Mode == DisplayMode.Pagination;
                btnNext.Visible = pagination;
                btnPrev.Visible = pagination;
                btnFirst.Visible = pagination;
                btnLast.Visible = pagination;
                lblPageInfo.Visible = pagination;
                _pageSizeComboBox.Visible = pagination;

                if (pagination)
                {
                    int totalPages = GetTotalPages();
                    lblPageInfo.Text = $"Page {_currentPage + 1} / {totalPages}";
                }
            }

            private int GetTotalPages()
            {
                if (FetchTotalCount == null)
                    return 1;

                int totalCount = FetchTotalCount();
                return (totalCount + _pageSize - 1) / _pageSize;
            }

            public void ApplyFilter(string filterExpression)
            {
                if (_sourceTable != null)
                {
                    _sourceTable.DefaultView.RowFilter = filterExpression;
                }
            }
        }
}
