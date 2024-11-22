using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace UtilityLib.Scheduler
{

    public class BackgroundPeriodicUpdater : IDisposable
    {
        private readonly Func<CancellationToken, Task> _updateAction;
        private readonly TimeSpan _interval;
        private CancellationTokenSource _cts;
        private Task _backgroundTask;

        public BackgroundPeriodicUpdater(Func<CancellationToken, Task> updateAction, TimeSpan interval)
        {
            _updateAction = updateAction ?? throw new ArgumentNullException(nameof(updateAction));
            _interval = interval > TimeSpan.Zero ? interval : throw new ArgumentOutOfRangeException(nameof(interval));
        }

        /// <summary>
        /// 定期更新を開始します。
        /// </summary>
        public void Start()
        {
            if (_backgroundTask != null && !_backgroundTask.IsCompleted)
            {
                throw new InvalidOperationException("The updater is already running.");
            }

            _cts = new CancellationTokenSource();
            _backgroundTask = RunPeriodicTask(_cts.Token);
        }

        /// <summary>
        /// 定期更新を停止します。
        /// </summary>
        public async Task StopAsync()
        {
            if (_cts == null) return;

            _cts.Cancel();
            try
            {
                if (_backgroundTask != null)
                {
                    await _backgroundTask;
                }
            }
            catch (OperationCanceledException)
            {
                // タスクがキャンセルされた場合は無視
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task RunPeriodicTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _updateAction(cancellationToken);
                }
                catch (Exception ex)
                {
                    // エラー処理（例: ログ記録）
                    Console.WriteLine($"Error in periodic task: {ex.Message}");
                }

                // 次の実行まで待機
                try
                {
                    await Task.Delay(_interval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // キャンセルされた場合はここで終了
                    break;
                }
            }
        }

        /// <summary>
        /// クラスを破棄します。
        /// </summary>
        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
        }
    }


}
