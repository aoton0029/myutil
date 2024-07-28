using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public class PeriodicTaskRunner
    {
        private readonly Action _action;
        private readonly TimeSpan _interval;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Progress<int> _progress;

        public PeriodicTaskRunner(Action action, TimeSpan interval, Progress<int> progress)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _interval = interval;
            _cancellationTokenSource = new CancellationTokenSource();
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () => await RunPeriodicTask(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task RunPeriodicTask(CancellationToken cancellationToken)
        {
            int progressValue = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _action();
                    progressValue = (progressValue + 10) % 100; // プログレスバーの値を更新
                    _progress.Report(progressValue);
                    await Task.Delay(_interval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // Task was cancelled, exit the loop
                    break;
                }
                catch (Exception ex)
                {
                    // Log or handle exception
                    MessageBox.Show($"Error in periodic task: {ex.Message}");
                }
            }
        }
    }

}
