using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public static class EnumerableExtentions
    {
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var item in self)
                action(item);
        }

        public static void ForEach<T>(this IEnumerable<T> self, Action<T, int> action)
        {
            self.Select((x, i) => new { x, i }).ForEach(x => action(x.x, x.i));
        }

        public static IEnumerable<T> ForEachLazy<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var item in self)
            {
                action(item);
                yield return item;
            }
        }

        public static IEnumerable<T> Diff<T>(this IEnumerable<T> self, IEnumerable<T> other)
        {
            var a = self.Except<T>(other);
            var b = other.Except<T>(self);
            return a.Union<T>(b);
        }

    }
}
