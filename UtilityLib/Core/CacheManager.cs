using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public class CacheManager<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache = new();
        private readonly TimeSpan _defaultExpiration;

        public CacheManager(TimeSpan? defaultExpiration = null)
        {
            _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(30);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;

            if (_cache.TryGetValue(key, out var cacheItem) && !cacheItem.IsExpired)
            {
                value = cacheItem.Value;
                return true;
            }

            // 期限切れアイテムを削除
            if (cacheItem != null && cacheItem.IsExpired)
            {
                _cache.TryRemove(key, out _);
            }

            return false;
        }

        public void Set(TKey key, TValue value, TimeSpan? expiration = null)
        {
            var cacheItem = new CacheItem<TValue>(value, expiration ?? _defaultExpiration);
            _cache.AddOrUpdate(key, cacheItem, (_, _) => cacheItem);
        }

        public void Remove(TKey key)
        {
            _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public async Task<TValue> GetOrAddAsync(TKey key, Func<Task<TValue>> valueFactory, TimeSpan? expiration = null)
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }

            value = await valueFactory();
            Set(key, value, expiration);
            return value;
        }

        private class CacheItem<T>
        {
            public T Value { get; }
            public DateTime ExpiresAt { get; }
            public bool IsExpired => DateTime.Now > ExpiresAt;

            public CacheItem(T value, TimeSpan expiration)
            {
                Value = value;
                ExpiresAt = DateTime.Now.Add(expiration);
            }
        }
    }
}
