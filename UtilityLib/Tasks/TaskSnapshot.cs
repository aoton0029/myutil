using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public interface ICredentialProvider
    {
        Task<bool> IsValidAppIdAsync(string appId);

        Task<string> GetAppPasswordAsync(string appId);

        Task<bool> IsAuthenticationDisabledAsync();
    }

    public interface ITask
    {
        /// <summary>
        /// Runs the task and blocks until it is complete.
        /// </summary>
        /// <param name="cancellationToken">Used to receive a signal (e.g. from another thread) when the user wants to cancel the task.</param>
        /// <param name="credentialProvider">Object used to retrieve credentials for specific <see cref="Uri"/>s on demand; can be <c>null</c>.</param>
        /// <param name="progress">Used to report back the task's progress (e.g. to another thread).</param>
        /// <exception cref="OperationCanceledException">The task was canceled from another thread.</exception>
        /// <exception cref="IOException">The task ended with <see cref="TaskState.IOError"/>.</exception>
        /// <exception cref="WebException">The task ended with <see cref="TaskState.WebError"/>.</exception>
        /// <seealso cref="ITaskHandler.RunTask"/>
        void Run(CancellationToken cancellationToken = default, ICredentialProvider? credentialProvider = null, IProgress<TaskSnapshot>? progress = null);
        string Name { get; }
        object? Tag { get; set; }
        bool CanCancel { get; }
    }

    public interface IResultTask<T> : ITask
    {
        /// <summary>
        /// The result of the task.
        /// </summary>
        /// <exception cref="InvalidOperationException">The task is not <see cref="TaskState.Complete"/>.</exception>
        T Result { get; }
    }

    [Serializable]
    public readonly record struct TaskSnapshot(TaskState State, bool UnitsByte = false, long UnitsProcessed = 0, long UnitsTotal = -1)
    {
        /// <summary>
        /// The progress of the task as a value between 0 and 1; -1 when unknown.
        /// </summary>
        public double Value =>
            UnitsTotal switch
            {
                -1 => -1,
                0 => 1,
                _ => (UnitsProcessed / (double)UnitsTotal)
            };

        /// <inheritdoc/>
        public override string ToString()
            => State switch
            {
                TaskState.Ready or TaskState.Started => "",
                //TaskState.Header => Resources.StateHeader,
                //TaskState.Data when UnitsTotal == -1 && UnitsProcessed == 0 => Resources.StateData,
                TaskState.Data when UnitsTotal == -1 => UnitsToString(UnitsProcessed),
                TaskState.Data => $"{UnitsToString(UnitsProcessed)} / {UnitsToString(UnitsTotal)}",
                //TaskState.Complete => Resources.StateComplete,
                //TaskState.IOError => Resources.StateIOError,
                _ => ""
            };

        private string UnitsToString(long units) => units.ToString();
            //=> UnitsByte
            //    ? units.FormatBytes()
            //    : units.ToString();
    }

    public sealed class ResultTask<T>([Localizable(true)] string name, Func<T> work, Action? cancellationCallback = null) : TaskBase, IResultTask<T>
    {
        /// <inheritdoc/>
        public override string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

        private T _result = default!;

        /// <inheritdoc/>
        public T Result
            => State == TaskState.Complete
                ? _result
                : throw new InvalidOperationException($"The task is in the state {State} and not Complete.");

        /// <summary>An optional callback to be called when cancellation is requested via a <see cref="CancellationToken"/>.</summary>
        private readonly Action? _cancellationCallback = cancellationCallback;

        /// <inheritdoc/>
        public override bool CanCancel => _cancellationCallback != null;

        /// <inheritdoc/>
        protected override bool UnitsByte => false;

        /// <inheritdoc/>
        protected override void Execute()
        {
            if (_cancellationCallback == null)
                _result = work();
            else
            {
                using (CancellationToken.Register(_cancellationCallback))
                    _result = work();
            }
        }
    }

    public static class ResultTask
    {
        /// <summary>
        /// Creates a new task that executes a callback and the provides a result. Only completion is reported, no intermediate progress.
        /// </summary>
        /// <param name="name">A name describing the task in human-readable form.</param>
        /// <param name="work">The code to be executed by the task that provides a result. May throw <see cref="WebException"/>, <see cref="IOException"/> or <see cref="OperationCanceledException"/>.</param>
        /// <param name="cancellationCallback">An optional callback to be called when cancellation is requested via a <see cref="CancellationToken"/>.</param>
        public static ResultTask<T> Create<T>([Localizable(true)] string name, Func<T> work, Action? cancellationCallback = null)
            => new(name, work, cancellationCallback);
    }


}
