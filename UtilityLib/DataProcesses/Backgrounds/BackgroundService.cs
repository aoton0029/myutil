using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses.Backgrounds
{
    public abstract class BackgroundService
    {
        private Task? _executingTask;
        private readonly CancellationTokenSource _cts = new();

        public void StartProcessing()
        {
            _executingTask = Task.Run(() => ExecuteAsync(_cts.Token));
        }

        public void StopProcessing()
        {
            _cts.Cancel();
            _executingTask?.Wait();
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
