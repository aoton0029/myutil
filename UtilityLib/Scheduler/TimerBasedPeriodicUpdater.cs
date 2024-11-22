using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Scheduler
{
    public class TimerBasedPeriodicUpdater : IDisposable
    {
        private readonly System.Threading.Timer _timer;
        private readonly Action _action;
        private readonly int _interval;
        private bool _isRunning;

        public TimerBasedPeriodicUpdater(Action action, int interval)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));
            _interval = interval;

            // タイマーを初期化（開始は後で）
            _timer = new System.Threading.Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 定期更新を開始します。
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _timer.Change(0, _interval); // 直ちに開始し、指定の間隔で繰り返す
        }

        /// <summary>
        /// 定期更新を停止します。
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _timer.Change(Timeout.Infinite, Timeout.Infinite); // タイマーを停止
            _isRunning = false;
        }

        private void Callback(object state)
        {
            try
            {
                RetryHelper.Retry.While()
            }
            catch (Exception ex)
            {
                // 必要に応じて例外をログに記録
                Console.WriteLine($"Error in periodic action: {ex.Message}");
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
