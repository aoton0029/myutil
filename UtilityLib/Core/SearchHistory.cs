using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public class SearchHistoryManager<T> where T : class
    {
        private readonly string _historyFilePath;
        private readonly int _maxHistoryItems;
        private readonly List<HistoryItem<T>> _historyItems = new();

        public SearchHistoryManager(string searchType, int maxItems = 20)
        {
            _maxHistoryItems = maxItems;

            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SearachAppSample", "SearchHistory");

            Directory.CreateDirectory(appDataPath);
            _historyFilePath = Path.Combine(appDataPath, $"{searchType}_history.json");

            LoadHistory();
        }

        private void LoadHistory()
        {
            if (File.Exists(_historyFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_historyFilePath);
                    var items = JsonSerializer.Deserialize<List<HistoryItem<T>>>
                        (json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (items != null)
                    {
                        _historyItems.Clear();
                        _historyItems.AddRange(items);
                    }
                }
                catch (Exception)
                {
                    // ファイル読み込みに失敗した場合は何もしない
                }
            }
        }

        private async Task SaveHistoryAsync()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_historyItems, options);
                await File.WriteAllTextAsync(_historyFilePath, json);
            }
            catch (Exception)
            {
                // ファイル書き込みに失敗した場合は何もしない
            }
        }

        public void AddSearchItem(T searchItem, string displayName = null)
        {
            // 同じ検索条件が既に存在する場合は削除
            _historyItems.RemoveAll(h =>
                JsonSerializer.Serialize(h.SearchItem) == JsonSerializer.Serialize(searchItem));

            // 新しい検索履歴を追加
            _historyItems.Insert(0, new HistoryItem<T>
            {
                SearchItem = searchItem,
                DisplayName = displayName ?? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                SearchDate = DateTime.Now
            });

            // 最大数を超えたら古いものを削除
            if (_historyItems.Count > _maxHistoryItems)
            {
                _historyItems.RemoveRange(_maxHistoryItems, _historyItems.Count - _maxHistoryItems);
            }

            // 非同期で保存
            _ = SaveHistoryAsync();
        }

        public List<HistoryItem<T>> GetSearchHistory()
        {
            return new List<HistoryItem<T>>(_historyItems);
        }

        public void ClearHistory()
        {
            _historyItems.Clear();
            _ = SaveHistoryAsync();
        }
    }

    public class HistoryItem<T>
    {
        public T SearchItem { get; set; }
        public string DisplayName { get; set; }
        public DateTime SearchDate { get; set; }
    }
}
