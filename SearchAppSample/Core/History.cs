using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAppSample.Core
{
    public class History<T>
    {
        private readonly Queue<T> _historyQueue;
        public int MaxCount { get; }

        public History(int maxCount)
        {
            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCount), "最大数は1以上である必要があります。");

            MaxCount = maxCount;
            _historyQueue = new Queue<T>(maxCount);
        }

        public void Add(T item)
        {
            if (_historyQueue.Count >= MaxCount)
            {
                _historyQueue.Dequeue(); // 古いエントリを削除
            }
            _historyQueue.Enqueue(item);
        }

        public IReadOnlyCollection<T> GetHistory()
        {
            return _historyQueue.ToArray();
        }

        public void Clear()
        {
            _historyQueue.Clear();
        }
    }
}
