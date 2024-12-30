using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    static class Utils
    {
        public static string ToStringDesc<T>(T aModel) where T : class
        {
            // クラスの型情報を取得
            var properties = aModel.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => Attribute.IsDefined(prop, typeof(DescriptionAttribute))) // Description属性が付いているものだけを選択
                .Select(prop =>
                {
                    var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(prop, typeof(DescriptionAttribute));
                    var description = attr?.Description ?? prop.Name; // 属性のDescriptionを取得
                    var value = prop.GetValue(aModel) ?? "null";        // プロパティの値を取得
                    return $"{description}: {value}";
                });

            return string.Join(", ", properties);
        }

        public static void AutoFillDefault<T>(this List<T> self, int count)
        {
            for (int x = 0; x < count; ++x)
            {
                self.Add(default(T));
            }
        }

        public static IEnumerable<int> To(this int fromNumber, int toNumber)
        {
            while (fromNumber < toNumber)
            {
                yield return fromNumber;
                fromNumber++;
            }
        }


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

        public static bool AllTrue(this IEnumerable<Func<bool>> conditions)
        {
            return conditions.All(condition => condition());
        }

        public static T GetNext<T>(this IList<T> list, T item)
        {
            int index = list.IndexOf(item);
            return index >= 0 && index < list.Count - 1 ? list[index + 1] : default;
        }

        public static T GetPrevious<T>(this IList<T> list, T item)
        {
            int index = list.IndexOf(item);
            return index > 0 ? list[index - 1] : default;
        }

        public static bool TryExecute(this Action action, Action<Exception> onError = null)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return false;
            }
        }

        public static void Repeat(this int times, Action action)
        {
            for (int i = 0; i < times; i++)
            {
                action();
            }
        }

        public static bool In<T>(this T value, params T[] values)
        {
            return values.Contains(value);
        }

        public static bool IsInRange<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        public static void SafeDispose(this IDisposable disposable)
        {
            try
            {
                disposable?.Dispose();
            }
            catch
            {
                // 無視
            }
        }

        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        public static void AddSafe<T>(this ConcurrentBag<T> bag, T item)
        {
            bag.Add(item);
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                return await task;
            throw new TimeoutException("The operation has timed out.");
        }

        public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> func)
        {
            var cache = new ConcurrentDictionary<T, TResult>();
            return arg => cache.GetOrAdd(arg, func);
        }

        public static bool IsValidIpAddress(this string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }

        public static TimeSpan MeasureExecutionTime(this Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        public static void CenterControl(Control parent, Control child)
        {
            child.Left = (parent.ClientSize.Width - child.Width) / 2;
            child.Top = (parent.ClientSize.Height - child.Height) / 2;
        }



    }
}
