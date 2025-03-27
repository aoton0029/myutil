using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSample
{
    public enum TaskState
    {
        Pending,
        Running,
        Completed,
        Cancelled,
        Failed
    }

    public abstract class TaskBase
    {
        public string Name { get; protected set; }
        public TaskState State { get; private set; } = TaskState.Pending;

        public Exception? LastError { get; protected set; }

        public DateTime? LastProgressTime { get; protected set; }
        public DateTime? StartTime { get; protected set; }
        public DateTime? EndTime { get; protected set; }
        public TimeSpan? Duration => StartTime.HasValue && EndTime.HasValue
            ? EndTime - StartTime
            : null;

        protected CancellationToken CancellationToken { get; private set; }
        protected IProgress<string>? ProgressReporter { get; private set; }

        protected TaskBase(string name)
        {
            Name = name;
        }

        public async Task RunAsync(CancellationToken cancellationToken, IProgress<string>? progress = null)
        {
            State = TaskState.Running;
            CancellationToken = cancellationToken;
            ProgressReporter = progress;
            StartTime = DateTime.Now;

            try
            {
                await ExecuteAsync();

                EndTime = DateTime.Now;

                State = cancellationToken.IsCancellationRequested
                    ? TaskState.Cancelled
                    : TaskState.Completed;
            }
            catch (OperationCanceledException)
            {
                EndTime = DateTime.Now;
                State = TaskState.Cancelled;
            }
            catch (Exception ex)
            {
                EndTime = DateTime.Now;
                LastError = ex;
                State = TaskState.Failed;
                throw;
            }
        }

        protected void ReportProgress(string message)
        {
            LastProgressTime = DateTime.Now;
            OnProgressChanged(message);
            ProgressReporter?.Report(message);
        }

        protected virtual void OnProgressChanged(string message) { }

        protected abstract Task ExecuteAsync();
    }

    public class TaskManagerService
    {
        private readonly SemaphoreSlim _concurrentLimiter;
        private readonly CancellationTokenSource _cts = new();
        private readonly ConcurrentBag<TaskBase> _allTasks = new();

        public event EventHandler<TaskBase>? TaskStarted;
        public event EventHandler<TaskBase>? TaskCompleted;
        public event EventHandler<(TaskBase Task, Exception Exception)>? TaskFailed;

        public TaskManagerService(int maxConcurrency = 1)
        {
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
                    await task.RunAsync(_cts.Token);
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

        public IEnumerable<(string Name, TimeSpan? Duration)> GetAllDurations()
        {
            return _allTasks.Select(t => (t.Name, t.Duration));
        }
    }


}
