using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public class BreadCrumb
    {
        private List<BreadCrumbItem> _items = new List<BreadCrumbItem>();

        // パンくずリストの項目を追加（順序指定なし - 最後尾に追加）
        public void AddItem(string title, Type pageType)
        {
            int order = _items.Count > 0 ? _items.Max(i => i.DisplayOrder) + 10 : 10;
            _items.Add(new BreadCrumbItem { Title = title, PageType = pageType, DisplayOrder = order });
        }

        // パンくずリストの項目を追加（順序指定あり）
        public void AddItem(string title, Type pageType, int displayOrder)
        {
            _items.Add(new BreadCrumbItem { Title = title, PageType = pageType, DisplayOrder = displayOrder });
            // 表示順でソート
            _items = _items.OrderBy(i => i.DisplayOrder).ToList();
        }

        // 現在地を設定
        public void SetCurrentPosition(Type pageType)
        {
            foreach (var item in _items)
            {
                item.IsCurrent = (item.PageType == pageType);
            }
        }

        // 項目一覧を取得（表示順でソート済み）
        public IReadOnlyList<BreadCrumbItem> Items => _items.OrderBy(i => i.DisplayOrder).ToList().AsReadOnly();

        // 特定の位置より後の項目を削除（戻った場合など）
        public void TruncateAfter(Type pageType)
        {
            // 対象項目の表示順を取得
            var targetItem = _items.FirstOrDefault(i => i.PageType == pageType);
            if (targetItem == null) return;

            // 表示順が対象より大きい項目を削除
            _items.RemoveAll(i => i.DisplayOrder > targetItem.DisplayOrder);
        }

        // 表示順を指定して項目を更新
        public void UpdateItemOrder(Type pageType, int newDisplayOrder)
        {
            var item = _items.FirstOrDefault(i => i.PageType == pageType);
            if (item != null)
            {
                item.DisplayOrder = newDisplayOrder;
                // 表示順でソート
                _items = _items.OrderBy(i => i.DisplayOrder).ToList();
            }
        }

        // 表示順を再設定（1から連番に振り直し）
        public void ReorderItems()
        {
            int order = 10;
            foreach (var item in _items.OrderBy(i => i.DisplayOrder))
            {
                item.DisplayOrder = order;
                order += 10;
            }
        }
    }

    public class BreadCrumbItem
    {
        public string Title { get; set; }
        public Type PageType { get; set; }
        public bool IsCurrent { get; set; }
        public int DisplayOrder { get; set; }
    }
}
