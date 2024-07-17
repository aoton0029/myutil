using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public static class TaskUtility
    {
        // Runs a task and captures any exceptions
        public static async Task RunSafeAsync(Func<Task> taskFunc, Action<Exception> onException = null)
        {
            try
            {
                await taskFunc();
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }
        }

        // Runs a task and returns a result, capturing any exceptions
        public static async Task<T> RunSafeAsync<T>(Func<Task<T>> taskFunc, Action<Exception> onException = null)
        {
            try
            {
                return await taskFunc();
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
                return default(T);
            }
        }   

        // Runs a task with a timeout
        public static async Task<bool> RunWithTimeoutAsync(Func<Task> taskFunc, int timeoutMilliseconds)
        {
            var task = taskFunc();
            if (await Task.WhenAny(task, Task.Delay(timeoutMilliseconds)) == task)
            {
                await task; // Propagate exception if task faulted
                return true; // Task completed within timeout
            }
            else
            {
                return false; // Timeout
            }
        }

        // Runs multiple tasks in parallel and waits for all to complete
        public static async Task RunAllAsync(IEnumerable<Func<Task>> taskFuncs, Action<Exception> onException = null)
        {
            var tasks = new List<Task>();
            foreach (var taskFunc in taskFuncs)
            {
                tasks.Add(RunSafeAsync(taskFunc, onException));
            }
            await Task.WhenAll(tasks);
        }

        // Runs multiple tasks in parallel and waits for all to complete, returning their results
        public static async Task<List<T>> RunAllAsync<T>(IEnumerable<Func<Task<T>>> taskFuncs, Action<Exception> onException = null)
        {
            var tasks = new List<Task<T>>();
            foreach (var taskFunc in taskFuncs)
            {
                tasks.Add(RunSafeAsync(taskFunc, onException));
            }
            var results = await Task.WhenAll(tasks);
            return new List<T>(results);
        }

        // Runs a task and retries on failure
        public static async Task RunWithRetryAsync(Func<Task> taskFunc, int retryCount, int delayMilliseconds = 1000, Action<Exception> onException = null)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    await taskFunc();
                    return; // Success, exit the method
                }
                catch (Exception ex)
                {
                    onException?.Invoke(ex);
                    if (i == retryCount - 1)
                    {
                        throw; // Re-throw if it's the last attempt
                    }
                    await Task.Delay(delayMilliseconds);
                }
            }
        }
    }

}

