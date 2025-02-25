using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpServer.Simulators
{
    class AsyncLogProcessor : IDisposable
    {
        private readonly string _logFilePath;
        private readonly ConcurrentQueue<string> _logQueue = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _logTask;

        public AsyncLogProcessor(string logFilePath = "server_log.txt")
        {
            _logFilePath = logFilePath;
            _logTask = Task.Run(ProcessLogQueueAsync);
        }

        public void Log(string message)
        {
            Debug.Print(message);
            string logMessage = $"{DateTime.Now}: {message}";
            _logQueue.Enqueue(logMessage);
        }

        private async Task ProcessLogQueueAsync()
        {
            //using StreamWriter writer = new(_logFilePath, append: true);
            while (!_cts.Token.IsCancellationRequested || !_logQueue.IsEmpty)
            {
                if (_logQueue.TryDequeue(out string logMessage))
                {
                    //await writer.WriteLineAsync(logMessage);
                    //await writer.FlushAsync();
                    Debug.Print(logMessage); // コンソールにも出力
                }
                else
                {
                    await Task.Delay(100); // ログがないときは待機
                }
            }
        }

        public async Task ShutdownAsync()
        {
            _cts.Cancel();
            await _logTask; // ログ処理タスクの完了を待つ
        }

        public void Dispose()
        {
            _cts.Cancel();
            _logTask.Wait();
        }
    }
}
