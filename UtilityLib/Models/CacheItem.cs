using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UtilityLib.Models
{
    public class CacheItem<T>
    {
        private T _value;
        private DateTime _lastUpdated;
        private TimeSpan _cacheDuration;
        private Func<T> _dataFetcher;
        private bool _forceRefresh; // 更新フラグ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataFetcher">データを取得するための関数</param>
        /// <param name="cacheDuration">キャッシュの有効期間</param>
        public CacheItem(Func<T> dataFetcher, TimeSpan cacheDuration)
        {
            _dataFetcher = dataFetcher ?? throw new ArgumentNullException(nameof(dataFetcher));
            _cacheDuration = cacheDuration;
            _lastUpdated = DateTime.MinValue;
            _forceRefresh = false;
        }

        /// <summary>
        /// キャッシュされた値を取得します。有効期限が切れている場合や更新フラグが立っている場合は再取得します。
        /// </summary>
        public T Value
        {
            get
            {
                if (IsExpired || _forceRefresh)
                {
                    Refresh();
                }
                return _value;
            }
        }

        /// <summary>
        /// キャッシュが有効期限切れかどうかを確認します。
        /// </summary>
        public bool IsExpired => DateTime.Now - _lastUpdated > _cacheDuration;

        /// <summary>
        /// データを強制的に再取得します。
        /// </summary>
        public void Refresh()
        {
            _value = _dataFetcher();
            _lastUpdated = DateTime.Now;
            _forceRefresh = false; // フラグをリセット
        }

        /// <summary>
        /// 更新フラグを設定します。
        /// </summary>
        public void SetForceRefresh()
        {
            _forceRefresh = true;
        }

        /// <summary>
        /// 更新フラグの状態を確認します。
        /// </summary>
        public bool IsForceRefresh => _forceRefresh;
    }

    public class JsonCache<T>
    {
        private readonly string _cacheFilePath;
        private readonly TimeSpan _cacheDuration;
        private DateTime _lastUpdated;
        private T _cachedData;

        public JsonCache(string cacheFilePath, TimeSpan cacheDuration)
        {
            _cacheFilePath = cacheFilePath;
            _cacheDuration = cacheDuration;
            _lastUpdated = DateTime.MinValue;
        }

        // キャッシュからデータを取得する
        public T Get(Func<T> fetchData)
        {
            if (IsCacheExpired() || _cachedData == null)
            {
                Console.WriteLine("Fetching new data...");
                _cachedData = fetchData();
                SaveCache(_cachedData);
            }
            else
            {
                Console.WriteLine("Using cached data...");
                _cachedData = LoadCache();
            }

            return _cachedData;
        }

        // キャッシュの有効期限を確認する
        private bool IsCacheExpired()
        {
            return !File.Exists(_cacheFilePath) || DateTime.Now - _lastUpdated > _cacheDuration;
        }

        // キャッシュを保存する
        private void SaveCache(T data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_cacheFilePath, json);
            _lastUpdated = DateTime.Now;
        }

        // キャッシュを読み込む
        private T LoadCache()
        {
            if (!File.Exists(_cacheFilePath))
                return default;

            var json = File.ReadAllText(_cacheFilePath);
            return JsonSerializer.Deserialize<T>(json);
        }
    }

}
