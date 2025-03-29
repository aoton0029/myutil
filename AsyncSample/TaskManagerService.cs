using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace AsyncSample
{


    public class TaskManagerService : IDisposable
    {
        private readonly SemaphoreSlim _concurrentLimiter;
        private readonly CancellationTokenSource _cts = new();
        private readonly ConcurrentBag<TaskBase> _allTasks = new();
        private readonly IProgress<TaskSnapshot>? _snapshotProgress;
        private bool _disposed = false;

        public event EventHandler<TaskBase>? TaskStarted;
        public event EventHandler<TaskBase>? TaskCompleted;
        public event EventHandler<(TaskBase Task, Exception Exception)>? TaskFailed;

        public TaskManagerService(int maxConcurrency = 1, IProgress<TaskSnapshot>? sharedProgress = null)
        {
            _snapshotProgress = sharedProgress;
            _concurrentLimiter = new SemaphoreSlim(maxConcurrency);
        }

        public void Enqueue(TaskBase task)
        {
            _allTasks.Add(task);

            _ = Task.Run(async () =>
            {
                await _concurrentLimiter.WaitAsync();

                try
                {
                    TaskStarted?.Invoke(this, task);
                    await task.RunAsync(_cts.Token, _snapshotProgress);
                    TaskCompleted?.Invoke(this, task);
                }
                catch (Exception ex)
                {
                    TaskFailed?.Invoke(this, (task, ex));
                }
                finally
                {
                    _concurrentLimiter.Release();
                }
            });
        }

        public void CancelAll() => _cts.Cancel();

        public IEnumerable<TaskBase> GetAllTasks() => _allTasks.ToList();

        public IEnumerable<(string TaskName, Exception Error)> GetFailedErrors()
        {
            return _allTasks
                .Where(t => t.State == TaskState.Failed && t.LastError != null)
                .Select(t => (t.Name, t.LastError!));
        }

        public void RemoveTasksByState(TaskState state)
        {
            var tasksToRemove = _allTasks.Where(t => t.State == state).ToList();
            foreach (var task in tasksToRemove)
            {
                _allTasks.TryTake(out var _); // ConcurrentBag は順序不定なので確実な削除ではない
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts.Cancel();
            _cts.Dispose();
            _concurrentLimiter.Dispose();
        }
    }


}
