using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Scheduler
{
    public class TimerBasedPeriodicUpdaterWithRetry : IDisposable
    {
        private readonly System.Threading.Timer _timer;
        private readonly Action _action;
        private readonly int _interval;
        private readonly int _maxRetryCount;
        private int _currentRetryCount;
        private bool _isRunning;
        private bool _isExecuting;

        public TimerBasedPeriodicUpdaterWithRetry(Action action, int interval, int maxRetryCount = 3)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));
            if (maxRetryCount < 0) throw new ArgumentOutOfRangeException(nameof(maxRetryCount));

            _interval = interval;
            _maxRetryCount = maxRetryCount;
            _timer = new System.Threading.Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 定期更新を開始します。
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _timer.Change(0, _interval);
        }

        /// <summary>
        /// 定期更新を停止します。
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _isRunning = false;
        }

        private void Callback(object state)
        {
            if (_isExecuting) return;

            _isExecuting = true;
            try
            {
                _currentRetryCount = 0; // リトライカウントをリセット
                ExecuteWithRetry();
            }
            finally
            {
                _isExecuting = false;
            }
        }

        private void ExecuteWithRetry()
        {
            while (_currentRetryCount <= _maxRetryCount)
            {
                try
                {
                    _action();
                    return; // 成功した場合は抜ける
                }
                catch (Exception ex)
                {
                    _currentRetryCount++;
                    Console.WriteLine($"Retry {_currentRetryCount}/{_maxRetryCount} failed: {ex.Message}");

                    if (_currentRetryCount > _maxRetryCount)
                    {
                        Console.WriteLine("Max retry attempts reached. Skipping this execution.");
                    }
                    else
                    {
                        Thread.Sleep(1000); // リトライ間隔を調整（例: 1秒待機）
                    }
                }
            }
        }

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        public void Dispose()
        {
            Stop();
            _timer.Dispose();
        }
    }
}
