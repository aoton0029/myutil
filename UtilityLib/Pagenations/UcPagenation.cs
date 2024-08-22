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
    public partial class UcPagenation : UserControl
    {
        private FlowLayoutPanel flowLayoutPanelPages;

        public int CurrentPage { get; private set; } = 1;
        public int TotalPages { get; private set; } = 1;
        public int ItemsPerPage { get; set; } = 10;
        public int TotalItems { get; private set; }

        public event EventHandler PageChanged;

        public UcPagenation()
        {
            InitializeComponent();

            flowLayoutPanelPages = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            this.Controls.Add(flowLayoutPanelPages);
            UpdatePagination();
        }

        public void SetTotalItems(int totalItems)
        {
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling(TotalItems / (double)ItemsPerPage);
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            lblPageInfo.Text = $"Page {CurrentPage} of {TotalPages}";
            btnPrevious.Enabled = CurrentPage > 1;
            btnNext.Enabled = CurrentPage < TotalPages;

            flowLayoutPanelPages.Controls.Clear();

            // 表示するページ番号の範囲を決定
            int maxPagesToShow = 7; // 表示する最大ページ数（必要に応じて調整）
            int startPage = Math.Max(1, CurrentPage - 3);
            int endPage = Math.Min(TotalPages, CurrentPage + 3);

            if (endPage - startPage < maxPagesToShow - 1)
            {
                startPage = Math.Max(1, endPage - (maxPagesToShow - 1));
            }

            // 最初のページを表示
            AddPageButton(1);

            if (startPage > 2)
            {
                AddEllipsis();
            }

            // 中間のページを表示
            for (int i = startPage; i <= endPage; i++)
            {
                AddPageButton(i);
            }

            if (endPage < TotalPages - 1)
            {
                AddEllipsis();
            }

            // 最後のページを表示
            if (TotalPages > 1)
            {
                AddPageButton(TotalPages);
            }
        }

        private void AddPageButton(int pageNumber)
        {
            var pageButton = new Button
            {
                Text = pageNumber.ToString(),
                Tag = pageNumber,
                Width = 30,
                Height = 30,
                Margin = new Padding(2),
                BackColor = pageNumber == CurrentPage ? Color.LightBlue : Color.LightGray
            };

            pageButton.Click += PageButton_Click;
            flowLayoutPanelPages.Controls.Add(pageButton);
        }

        private void PageButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                int selectedPage = (int)button.Tag;
                if (selectedPage != CurrentPage)
                {
                    CurrentPage = selectedPage;
                    PageChanged?.Invoke(this, EventArgs.Empty);
                    UpdatePagination();
                }
            }
        }

        private void AddEllipsis()
        {
            var ellipsisLabel = new Label
            {
                Text = "...",
                AutoSize = true,
                Margin = new Padding(2, 8, 2, 8)
            };

            flowLayoutPanelPages.Controls.Add(ellipsisLabel);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                PageChanged?.Invoke(this, EventArgs.Empty);
                UpdatePagination();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                PageChanged?.Invoke(this, EventArgs.Empty);
                UpdatePagination();
            }
        }
    }
}
