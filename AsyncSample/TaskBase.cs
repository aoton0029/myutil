using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

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
        protected IProgress<TaskSnapshot>? SnapshotReporter { get; private set; }


        protected TaskBase(string name, IProgress<TaskSnapshot>? progress)
        {
            Name = name;
            SnapshotReporter = progress;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            State = TaskState.Running;
            CancellationToken = cancellationToken;
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
                ReportProgress($"タスク実行中にエラー発生: {ex.Message}");
                EndTime = DateTime.Now;
                LastError = ex;
                State = TaskState.Failed;
                throw;
            }
        }

        protected void ReportProgress(string? message = null, double? percent = null, Exception? error = null)
        {
            LastProgressTime = DateTime.Now;

            SnapshotReporter?.Report(new TaskSnapshot
            {
                Name = Name,
                State = State,
                ProgressPercentage = percent,
                Message = message,
                Timestamp = DateTime.Now,
                Error = error
            });
        }

        protected abstract Task ExecuteAsync();
    }
}
