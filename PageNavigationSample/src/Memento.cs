using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public class Memento<T>
    {
        public T State { get; }

        public Memento(T state)
        {
            // ディープコピーをサポート
            State = DeepCopy(state);
        }

        private T DeepCopy(T obj)
        {
            if (obj == null) return default;

            var json = System.Text.Json.JsonSerializer.Serialize(obj);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }
}
