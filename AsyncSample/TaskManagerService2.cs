using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AsyncSample
{
    /*
    public class TaskManagerService2 : IDisposable
    {
        private readonly Channel<TaskBase> _channel;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _consumerTask;

        public event EventHandler<TaskBase>? TaskStarted;
        public event EventHandler<TaskBase>? TaskCompleted;
        public event EventHandler<(TaskBase Task, Exception Exception)>? TaskFailed;

        public TaskManagerService2()
        {
            // バッファ付きチャンネル（無限）
            _channel = Channel.CreateUnbounded<TaskBase>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            // コンシューマ開始
            _consumerTask = Task.Run(ProcessQueueAsync);
        }

        public void Enqueue(TaskBase task)
        {
            _channel.Writer.TryWrite(task);
        }

        private async Task ProcessQueueAsync()
        {
            while (await _channel.Reader.WaitToReadAsync(_cts.Token))
            {
                while (_channel.Reader.TryRead(out var task))
                {
                    try
                    {
                        TaskStarted?.Invoke(this, task);
                        await task.RunAsync(_cts.Token);
                        TaskCompleted?.Invoke(this, task);
                    }
                    catch (Exception ex)
                    {
                        TaskFailed?.Invoke(this, (task, ex));
                    }
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _channel.Writer.Complete();
            _consumerTask.Wait();
            _cts.Dispose();
        }
    }
    */
}
