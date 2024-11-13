using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Threadings
{
    public static class TaskCompletionSourceUtility
    {
        /// <summary>
        /// Sets the state of the specified <see cref="TaskCompletionSource{TResult}.Task"/> to that of the specified <see cref="Task{TResult}"/>.
        /// </summary>
        /// <param name="source">A <see cref="TaskCompletionSource{TResult}"/> that will have its Task's status set.</param>
        /// <param name="task">The <see cref="Task{TResult}"/> that supplies the result or exception for the <see cref="TaskCompletionSource{TResult}"/>.</param>
        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> source, Task<TResult> task)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (task == null)
                throw new ArgumentNullException("task");

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    source.SetResult(task.Result);
                    break;

                case TaskStatus.Faulted:
                    source.SetException(task.Exception.InnerExceptions);
                    break;

                case TaskStatus.Canceled:
                    source.SetCanceled();
                    break;

                default:
                    throw new InvalidOperationException("The task was not completed.");
            }
        }
    }

    public static class TaskUtility
    {
        public static IAsyncResult CreateAsyncResult<T>(this Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            // create result object that can hold the asynchronously-computed value
            TaskCompletionSource<T> result = new TaskCompletionSource<T>(state);

            // set the result (or failure) when the value is known
            task.ContinueWith(t =>
            {
                result.SetFromTask(t);
                if (callback != null)
                    callback(result.Task);
            });

            // the result's task functions as the IAsyncResult APM return value
            return result.Task;
        }
    }

    public class ThreadUtils 
    {
        public static Thread StartAsync(ThreadStart execute, [Localizable(false)] string? name = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            var thread = new Thread(execute) { Name = name };
            thread.Start();
            return thread;
        }

        public static Thread StartBackground(ThreadStart execute, [Localizable(false)] string? name = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            var thread = new Thread(execute) { Name = name, IsBackground = true };
            thread.Start();
            return thread;
        }

        public static void RunSta(Action execute)
        {
            Exception? error = null;
            var thread = new Thread(new ThreadStart(delegate
            {
                try
                {
                    execute();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            //error?.Rethrow();
        }

        public static T RunSta<T>(Func<T> execute)
        {
            T result = default!;
            Exception? error = null;
            var thread = new Thread(new ThreadStart(delegate
            {
                try
                {
                    result = execute();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            //error?.Rethrow();
            return result;
        }

        public static void RunTask(Func<Task> action)
        {
            var synchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                action().Wait();
            }
            catch (AggregateException ex)
            {
                throw;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }

        }

        public static T RunTask<T>(Func<Task<T>> action)
        {
            var synchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                return action().Result;
            }
            catch (AggregateException ex)
            {
                throw;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }
        }
    }
}
