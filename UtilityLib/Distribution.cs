using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public enum DistributionMode
    {
        Average,
        FixedPerGroup
    }
    public enum RemainderDistributionMode
    {
        AtStart,
        AtEnd
    }
    public class DistributionConfig<TItem>
    {
        public Func<TItem, object> SortKeySelector { get; set; } = _ => 0;
        public bool SortAscending { get; set; } = true;
        public int MaxItemsPerGroup { get; set; } = 10;
        public DistributionMode Mode { get; set; } = DistributionMode.Average;
        public Dictionary<string, int>? FixedGroupItemCounts { get; set; }
        public RemainderDistributionMode RemainderMode { get; set; } = RemainderDistributionMode.AtStart;
    }

    public class DistributionManager<TItem>
    {
        private readonly List<TItem> _sourceItems = new();
        private readonly List<List<TItem>> _groups = new();
        private DistributionConfig<TItem> _config = new();

        public void LoadItems(IEnumerable<TItem> items)
        {
            _sourceItems.Clear();
            _sourceItems.AddRange(items);
        }

        public void SetConfig(DistributionConfig<TItem> config)
        {
            _config = config;
        }

        public void Distribute()
        {
            if (_config.SortKeySelector == null)
                throw new InvalidOperationException("SortKeySelector is not set.");

            var sortedItems = _config.SortAscending
                ? _sourceItems.OrderBy(_config.SortKeySelector).ToList()
                : _sourceItems.OrderByDescending(_config.SortKeySelector).ToList();

            _groups.Clear();

            if (_config.Mode == DistributionMode.Average)
            {
                DistributeAverage(sortedItems);
            }
            else if (_config.Mode == DistributionMode.FixedPerGroup)
            {
                DistributeFixedPerGroup(sortedItems);
            }
        }

        private void DistributeAverage(List<TItem> sortedItems)
        {
            int groupSize = _config.MaxItemsPerGroup;
            int totalItems = sortedItems.Count;
            int groupCount = totalItems / groupSize;
            int remainder = totalItems % groupSize;

            if (groupCount == 0) groupCount = 1;

            var groupSizes = Enumerable.Repeat(groupSize, groupCount).ToList();

            if (remainder > 0)
            {
                if (_config.RemainderMode == RemainderDistributionMode.AtStart)
                {
                    for (int i = 0; i < remainder; i++) groupSizes[i]++;
                }
                else
                {
                    for (int i = groupSizes.Count - remainder; i < groupSizes.Count; i++) groupSizes[i]++;
                }
            }

            int currentIndex = 0;
            for (int i = 0; i < groupSizes.Count; i++)
            {
                var group = sortedItems.Skip(currentIndex).Take(groupSizes[i]).ToList();
                _groups.Add(group);
                currentIndex += groupSizes[i];
            }
        }

        private void DistributeFixedPerGroup(List<TItem> sortedItems)
        {
            if (_config.FixedGroupItemCounts == null)
                throw new InvalidOperationException("FixedGroupItemCounts is not set.");

            int index = 0;
            foreach (var kvp in _config.FixedGroupItemCounts)
            {
                var group = sortedItems.Skip(index).Take(kvp.Value).ToList();
                _groups.Add(group);
                index += kvp.Value;
                if (index >= sortedItems.Count)
                    break;
            }
        }

        public List<List<TItem>> GetGroups()
        {
            return _groups;
        }
    }

    public class PageModel<TItem>
    {
        public int PageNumber { get; set; }
        public string PageName { get; set; } = string.Empty;
        public List<TItem> Items { get; set; } = new();

        public int RowCount { get; set; } = 1;
        public int ColumnCount { get; set; } = 1;

        public int MaxItems => RowCount * ColumnCount;

        public int ItemCount => Items.Count;

        public bool IsFull => ItemCount >= MaxItems;
    }

    public static class PageModelHelper
    {
        public static List<PageModel<TItem>> CreatePages<TItem>(
            List<List<TItem>> groups, int rowCount, int columnCount)
        {
            var pages = new List<PageModel<TItem>>();
            int pageSize = rowCount * columnCount;

            int pageNumber = 1;
            foreach (var group in groups)
            {
                int index = 0;
                while (index < group.Count)
                {
                    var page = new PageModel<TItem>
                    {
                        PageNumber = pageNumber++,
                        PageName = $"Page {pageNumber - 1}",
                        RowCount = rowCount,
                        ColumnCount = columnCount
                    };

                    page.Items.AddRange(group.Skip(index).Take(pageSize));
                    pages.Add(page);

                    index += pageSize;
                }
            }

            return pages;
        }
    }


}
