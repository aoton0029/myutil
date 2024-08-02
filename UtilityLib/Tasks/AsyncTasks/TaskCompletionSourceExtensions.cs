using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks.AsyncTasks
{
    public static class TaskCompletionSourceExtensions
    {
        public static bool TryCompleteFromCompletedTask<TResult, TSourceResult>(this TaskCompletionSource<TResult> @this, Task<TSourceResult> task) where TSourceResult : TResult
        {
            if (task.IsFaulted)
                return @this.TrySetException(task.Exception.InnerExceptions);
            if (task.IsCanceled)
            {
                try
                {
                    task.WaitAndUnwrapException();
                }
                catch (OperationCanceledException exception)
                {
                    var token = exception.CancellationToken;
                    return token.IsCancellationRequested ? @this.TrySetCanceled(token) : @this.TrySetCanceled();
                }
            }
            return @this.TrySetResult(task.Result);
        }

        public static bool TryCompleteFromCompletedTask<TResult>(this TaskCompletionSource<TResult> @this, Task task, Func<TResult> resultFunc)
        {
            if (task.IsFaulted)
                return @this.TrySetException(task.Exception.InnerExceptions);
            if (task.IsCanceled)
            {
                try
                {
                    task.WaitAndUnwrapException();
                }
                catch (OperationCanceledException exception)
                {
                    var token = exception.CancellationToken;
                    return token.IsCancellationRequested ? @this.TrySetCanceled(token) : @this.TrySetCanceled();
                }
            }
            return @this.TrySetResult(resultFunc());
        }

        public static TaskCompletionSource<TResult> CreateAsyncTaskSource<TResult>()
        {
            return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
