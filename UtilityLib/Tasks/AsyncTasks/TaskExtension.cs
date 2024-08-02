using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks.AsyncTasks
{
    public static class TaskExtensions
    {
        public static Task WaitAsync(this Task @this, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return @this;
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);
            return DoWaitAsync(@this, cancellationToken);
        }

        private static async Task DoWaitAsync(Task task, CancellationToken cancellationToken)
        {
            using (var cancelTaskSource = new CancellationTokenTaskSource<object>(cancellationToken))
                await (await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public static Task<TResult> WaitAsync<TResult>(this Task<TResult> @this, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return @this;
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResult>(cancellationToken);
            return DoWaitAsync(@this, cancellationToken);
        }

        private static async Task<TResult> DoWaitAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken)
        {
            using (var cancelTaskSource = new CancellationTokenTaskSource<TResult>(cancellationToken))
                return await (await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public static Task<Task> WhenAny(this IEnumerable<Task> @this, CancellationToken cancellationToken)
        {
            return Task.WhenAny(@this).WaitAsync(cancellationToken);
        }

        public static Task<Task> WhenAny(this IEnumerable<Task> @this)
        {
            return Task.WhenAny(@this);
        }

        public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this, CancellationToken cancellationToken)
        {
            return Task.WhenAny(@this).WaitAsync(cancellationToken);
        }

        public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this)
        {
            return Task.WhenAny(@this);
        }

        public static Task WhenAll(this IEnumerable<Task> @this)
        {
            return Task.WhenAll(@this);
        }

        public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> @this)
        {
            return Task.WhenAll(@this);
        }

        public static async void Ignore(this Task @this)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            try
            {
                await @this.ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        public static async void Ignore<T>(this Task<T> @this)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            try
            {
                await @this.ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        public static List<Task<T>> OrderByCompletion<T>(this IEnumerable<Task<T>> @this)
        {
            var taskArray = @this.ToArray();
            var numTasks = taskArray.Length;
            var tcs = new TaskCompletionSource<T>[numTasks];
            var ret = new List<Task<T>>(numTasks);

            var lastIndex = -1;
            Action<Task<T>> continuation = task =>
            {
                var index = Interlocked.Increment(ref lastIndex);
                tcs[index].TryCompleteFromCompletedTask(task);
            };

            for (var i = 0; i != numTasks; ++i)
            {
                tcs[i] = new TaskCompletionSource<T>();
                ret.Add(tcs[i].Task);
                taskArray[i].ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            return ret;
        }

        public static List<Task> OrderByCompletion(this IEnumerable<Task> @this)
        {
            // Reify the source task sequence. TODO: better reification.
            var taskArray = @this.ToArray();

            // Allocate a TCS array and an array of the resulting tasks.
            var numTasks = taskArray.Length;
            var tcs = new TaskCompletionSource<object?>[numTasks];
            var ret = new List<Task>(numTasks);

            // As each task completes, complete the next tcs.
            var lastIndex = -1;
            // ReSharper disable once ConvertToLocalFunction
            Action<Task> continuation = task =>
            {
                var index = Interlocked.Increment(ref lastIndex);
                tcs[index].TryCompleteFromCompletedTask(task, NullResultFunc);
            };

            // Fill out the arrays and attach the continuations.
            for (var i = 0; i != numTasks; ++i)
            {
                tcs[i] = new TaskCompletionSource<object?>();
                ret.Add(tcs[i].Task);
                taskArray[i].ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            return ret;
        }

        private static Func<object?> NullResultFunc { get; } = () => null;
    }

}
