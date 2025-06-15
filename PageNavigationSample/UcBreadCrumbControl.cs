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
    public partial class UcBreadCrumbControl : UserControl
    {
        private readonly BreadCrumb _breadCrumb;

        // デザインカスタマイズのためのプロパティ
        [Category("Appearance")]
        public Font ItemFont { get; set; } = new Font("Segoe UI", 9F, FontStyle.Regular);

        [Category("Appearance")]
        public Font CurrentItemFont { get; set; } = new Font("Segoe UI", 9F, FontStyle.Bold);

        [Category("Appearance")]
        public Color ItemForeColor { get; set; } = Color.Black;

        [Category("Appearance")]
        public Color CurrentItemForeColor { get; set; } = Color.Blue;

        [Category("Appearance")]
        public Color SeparatorColor { get; set; } = Color.Gray;

        [Category("Appearance")]
        public string SeparatorText { get; set; } = " > ";

        [Category("Behavior")]
        public int ItemSpacing { get; set; } = 5;

        // クリックで移動機能を有効にするかどうかのプロパティ
        [Category("Behavior")]
        [Description("クリックでの移動機能を有効にするかどうかを設定します")]
        public bool EnableNavigation { get; set; } = true;

        // パンくずリストがクリックされた時のイベント
        public event EventHandler<BreadCrumbClickEventArgs> BreadCrumbItemClick;

        public UcBreadCrumbControl(BreadCrumb breadCrumb)
        {
            InitializeComponent();
            _breadCrumb = breadCrumb;
            flowLayoutPanel1.Padding = new Padding(3);

            // 水平スクロールを有効にする
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = false;
        }

        // オーバーロードしたコンストラクタでEnableNavigationを設定できるようにする
        public UcBreadCrumbControl(BreadCrumb breadCrumb, bool enableNavigation) : this(breadCrumb)
        {
            EnableNavigation = enableNavigation;
        }

        // パンくずリストの表示を更新する
        public void UpdateBreadCrumbDisplay()
        {
            flowLayoutPanel1.Controls.Clear();

            var items = _breadCrumb.Items.ToList();
            if (items.Count == 0)
            {
                return;
            }

            // 常にすべての項目を表示する
            DisplayAllItems(items);
        }

        // すべての項目を表示
        private void DisplayAllItems(List<BreadCrumbItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                AddItemToPanel(items[i]);

                // 最後の項目以外はセパレーターを追加
                if (i < items.Count - 1)
                {
                    AddSeparatorToPanel();
                }
            }
        }

        // パンくずリスト項目をパネルに追加
        private void AddItemToPanel(BreadCrumbItem item)
        {
            var linkLabel = new LinkLabel
            {
                Text = item.Title,
                Font = item.IsCurrent ? CurrentItemFont : ItemFont,
                ForeColor = item.IsCurrent ? CurrentItemForeColor : ItemForeColor,
                LinkColor = item.IsCurrent ? CurrentItemForeColor : ItemForeColor,
                ActiveLinkColor = CurrentItemForeColor,
                VisitedLinkColor = ItemForeColor,
                AutoSize = true,
                Tag = item,
                LinkBehavior = LinkBehavior.HoverUnderline
            };

            // 現在の項目の場合はリンクを無効化
            if (item.IsCurrent)
            {
                linkLabel.LinkBehavior = LinkBehavior.NeverUnderline;
                linkLabel.DisabledLinkColor = CurrentItemForeColor;
                linkLabel.Enabled = false;
            }
            else if (EnableNavigation) // EnableNavigationがtrueの場合のみクリックイベントを設定
            {
                // クリックイベントを設定
                linkLabel.Click += LinkLabel_Click;
            }
            else
            {
                // クリック機能を無効化した場合のスタイル設定
                linkLabel.LinkBehavior = LinkBehavior.NeverUnderline;
                linkLabel.Cursor = Cursors.Default;
            }

            flowLayoutPanel1.Controls.Add(linkLabel);
        }

        // セパレーターをパネルに追加
        private void AddSeparatorToPanel()
        {
            var separatorLabel = new Label
            {
                Text = SeparatorText,
                ForeColor = SeparatorColor,
                AutoSize = true,
                Margin = new Padding(ItemSpacing, 0, ItemSpacing, 0)
            };
            flowLayoutPanel1.Controls.Add(separatorLabel);
        }

        private void LinkLabel_Click(object sender, EventArgs e)
        {
            if (sender is LinkLabel linkLabel && linkLabel.Tag is BreadCrumbItem item)
            {
                BreadCrumbItemClick?.Invoke(this, new BreadCrumbClickEventArgs(item));
            }
        }
    }

    // パンくずリストのクリックイベント引数
    public class BreadCrumbClickEventArgs : EventArgs
    {
        public BreadCrumbItem ClickedItem { get; }

        public BreadCrumbClickEventArgs(BreadCrumbItem clickedItem)
        {
            ClickedItem = clickedItem;
        }
    }
}
