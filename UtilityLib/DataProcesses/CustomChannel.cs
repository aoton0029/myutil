using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses
{
    public class CustomChannel<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly int _capacity;
        private bool _isCompleted = false;
        private readonly object _lock = new object();
        private readonly SemaphoreSlim _itemsAvailable = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _spaceAvailable;

        public CustomChannel(int capacity = 100)
        {
            _capacity = capacity;
            _spaceAvailable = new SemaphoreSlim(capacity);
        }

        // データを書き込む (Producer)
        public async Task WriteAsync(T item, CancellationToken cancellationToken = default)
        {
            await _spaceAvailable.WaitAsync(cancellationToken); // 空きスペースができるまで待つ
            lock (_lock)
            {
                if (_isCompleted) throw new InvalidOperationException("Channel is completed.");
                _queue.Enqueue(item);
            }
            _itemsAvailable.Release(); // アイテムが利用可能であることを通知
        }

        // データを読む (Consumer)
        public async Task<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            await _itemsAvailable.WaitAsync(cancellationToken); // データが来るまで待つ
            lock (_lock)
            {
                if (_queue.Count == 0 && _isCompleted) throw new InvalidOperationException("Channel is empty and completed.");
                var item = _queue.Dequeue();
                _spaceAvailable.Release(); // 空きスペースを増やす
                return item;
            }
        }

        // 書き込みの終了を宣言
        public void Complete()
        {
            lock (_lock)
            {
                _isCompleted = true;
            }
        }

        internal void Write(Backgrounds.DataWithKey data)
        {
            throw new NotImplementedException();
        }

        // チャネルが閉じていて、データもないか確認
        public bool IsCompleted
        {
            get
            {
                lock (_lock)
                {
                    return _isCompleted && _queue.Count == 0;
                }
            }
        }
    }
}
