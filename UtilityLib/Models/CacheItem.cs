using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
