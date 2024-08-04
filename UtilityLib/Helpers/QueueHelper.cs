using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Helpers
{
    internal static class QueueHelper
    {
        public static Queue<T> Push<T>(this Queue<T> queue, T item)
        {
            queue.Enqueue(item);
            return queue;
        }

        public static Queue<T> Pop<T>(this Queue<T> queue)
        {
            if (queue.Count > 0)
                _ = queue.Dequeue();
            return queue;
        }

        public static bool IsEmpty<T>(this Queue<T> queue) => queue.Count == 0;

        public static bool IsNotEmpty<T>(this Queue<T> queue) => queue.Count > 0;

        public static Queue<T> ForEach<T>(
            this Queue<T> queue,
            Action<T> action,
            bool reappend = false,
            object? locker = null
        )
        {
            Queue<T> func()
            {
                var count = queue.Count;
                while (count > 0)
                {
                    var item = queue.Dequeue();
                    action.Invoke(item);
                    --count;
                    if (reappend) queue.Enqueue(item);
                }
                return queue;
            }

            if (locker is not null)
            {
                lock (locker)
                {
                    return func();
                }
            }
            else return func();
        }

        public static async Task<Queue<T>> ForEachAsync<T>(
            this Queue<T> queue,
            Action<T> action,
            bool reappend = false,
            CancellationToken token = default
        )
        {
            Queue<T> func()
            {
                var count = queue.Count;
                while (count > 0)
                {
                    var item = queue.Dequeue();
                    action.Invoke(item);
                    --count;
                    if (reappend) queue.Enqueue(item);
                }
                return queue;
            }

            return await Task.Run(func, token);
        }
    }
}
