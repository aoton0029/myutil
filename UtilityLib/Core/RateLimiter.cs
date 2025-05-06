using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public class RateLimiter
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<TaskCompletionSource<bool>> _queue = new();
        private readonly int _maxParallel;
        private readonly TimeSpan _interval;
        private DateTime _lastExecutionTime = DateTime.MinValue;

        public RateLimiter(int maxParallel, TimeSpan interval)
        {
            _maxParallel = maxParallel;
            _interval = interval;
            _semaphore = new SemaphoreSlim(maxParallel, maxParallel);
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                // 前回の実行から指定間隔が経過するまで待機
                var timeSinceLastExecution = DateTime.Now - _lastExecutionTime;
                if (timeSinceLastExecution < _interval)
                {
                    await Task.Delay(_interval - timeSinceLastExecution, cancellationToken);
                }

                _lastExecutionTime = DateTime.Now;
                return await action();
            }
            finally
            {
                _semaphore.Release();

                // キューに待機中のタスクがあれば一つ解放
                if (_queue.TryDequeue(out var tcs))
                {
                    tcs.SetResult(true);
                }
            }
        }

        public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(async () =>
            {
                await action();
                return true;
            }, cancellationToken);
        }
    }
}
