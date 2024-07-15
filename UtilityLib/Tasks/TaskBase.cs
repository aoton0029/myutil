using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public enum TaskState
    {
        Ready,
        Started,
        Header,
        Data,
        Complete,
        IOError,
        Canceled
    }

    public abstract class TaskBase : MarshalByRefObject
    {
        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public object? Tag { get; set; }

        /// <inheritdoc/>
        public virtual bool CanCancel => true;

        /// <summary>
        /// Indicates whether this task should prevent the system from entering idle mode.
        /// </summary>
        protected virtual bool PreventIdle => true;

        /// <summary>Signaled when the user wants to cancel the task execution.</summary>
        protected CancellationToken CancellationToken;

        /// <summary>Used to report back the task's progress.</summary>
        private IProgress<TaskSnapshot>? _progress;

        /// <summary>Used to retrieve credentials for specific <see cref="Uri"/>s on demand; can be <c>null</c>.</summary>
        protected ICredentialProvider? CredentialProvider;

        /// <inheritdoc/>
        public void Run(CancellationToken cancellationToken = default, ICredentialProvider? credentialProvider = null, IProgress<TaskSnapshot>? progress = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CancellationToken = cancellationToken;
            _progress = progress;
            CredentialProvider = credentialProvider;

            State = TaskState.Started;

            try
            {
                Execute();
            }
            catch (OperationCanceledException)
            {
                State = TaskState.Canceled;
                throw;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                State = TaskState.IOError;
                throw;
            }

            State = TaskState.Complete;

            cancellationToken.ThrowIfCancellationRequested();
        }

        #region Progress
        private TaskState _state;

        protected internal TaskState State { get => _state; protected set => value.To(ref _state, OnProgressChanged); }

        protected abstract bool UnitsByte { get; }

        private long _unitsProcessed;

        protected long UnitsProcessed { get => _unitsProcessed; set => value.To(ref _unitsProcessed, OnProgressChangedThrottled); }

        private long _unitsTotal = -1;

        protected long UnitsTotal { get => _unitsTotal; set => value.To(ref _unitsTotal, OnProgressChanged); }

        private void OnProgressChanged() => _progress?.Report(new(_state, UnitsByte, _unitsProcessed, _unitsTotal));

        private DateTime _lastProgress;
        private static readonly TimeSpan _progressRate = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Informs the caller of the current progress, if a callback was registered. Limits the rate of progress updates.
        /// </summary>
        private void OnProgressChangedThrottled()
        {
            if (_progress == null) return;

            var now = DateTime.Now;
            if ((now - _lastProgress) < _progressRate) return;

            _progress.Report(new(_state, UnitsByte, _unitsProcessed, _unitsTotal));
            _lastProgress = now;
        }
        #endregion

        protected abstract void Execute();
    }
}
