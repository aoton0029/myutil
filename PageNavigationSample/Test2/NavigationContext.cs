using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Test2
{
    public class NavigationContext
    {
        public Type CurrentPage { get; set; }
        public Type DefaultNextPage { get; set; }
        public object? Parameter { get; set; }

        // ページ間で共有されるデータ（フォームの値、進捗、フラグ等）
        public Dictionary<string, object> SharedData { get; set; } = new();

        public T? Get<T>(string key)
        {
            return SharedData.TryGetValue(key, out var value) ? (T?)value : default;
        }

        public void Set<T>(string key, T value)
        {
            SharedData[key] = value!;
        }
    }
}
