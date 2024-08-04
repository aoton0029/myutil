using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public class AsyncExecutor
    {
        /// <summary>
        /// Creates a new instance of asynchronous executor.
        /// </summary>
        public AsyncExecutor()
        { }

        /// <summary>
        /// Executes a specified task in an asynchronous manner, waiting for its completion.
        /// </summary>
        /// <param name="task">Task to execute.</param>
        public void Execute(Task task)
        {
            // create state object
            var taskState = new StateRef<object>(new AutoResetEvent(false));

            // queue a task and wait for it to finish executing
            task.ContinueWith(TaskCompletionHandler, taskState);
            taskState.Lock.WaitOne();

            // check for and rethrow any exceptions
            if (taskState.Exception != null)
                throw taskState.Exception;

            // completion method
            void TaskCompletionHandler(Task t, object state)
            {
                // retrieve state data
                var stateRef = state as StateRef<object>;

                // retrieve any exceptions or cancellation status
                if (t.IsFaulted)
                {
                    if (t.Exception.InnerExceptions.Count == 1) // unwrap if 1
                        stateRef.Exception = t.Exception.InnerException;
                    else
                        stateRef.Exception = t.Exception;
                }
                else if (t.IsCanceled)
                {
                    stateRef.Exception = new TaskCanceledException(t);
                }

                // signal that the execution is done
                stateRef.Lock.Set();
            }
        }

        /// <summary>
        /// Executes a specified task in an asynchronous manner, waiting for its completion, and returning the result.
        /// </summary>
        /// <typeparam name="T">Type of the Task's return value.</typeparam>
        /// <param name="task">Task to execute.</param>
        /// <returns>Task's result.</returns>
        public T Execute<T>(Task<T> task)
        {
            // create state object
            var taskState = new StateRef<T>(new AutoResetEvent(false));

            // queue a task and wait for it to finish executing
            task.ContinueWith(TaskCompletionHandler, taskState);
            taskState.Lock.WaitOne();

            // check for and rethrow any exceptions
            if (taskState.Exception != null)
                throw taskState.Exception;

            // return the result, if any
            if (taskState.HasResult)
                return taskState.Result;

            // throw exception if no result
            throw new Exception("Task returned no result.");

            // completion method
            void TaskCompletionHandler(Task<T> t, object state)
            {
                // retrieve state data
                var stateRef = state as StateRef<T>;

                // retrieve any exceptions or cancellation status
                if (t.IsFaulted)
                {
                    if (t.Exception.InnerExceptions.Count == 1) // unwrap if 1
                        stateRef.Exception = t.Exception.InnerException;
                    else
                        stateRef.Exception = t.Exception;
                }
                else if (t.IsCanceled)
                {
                    stateRef.Exception = new TaskCanceledException(t);
                }

                // return the result from the task, if any
                if (t.IsCompleted && !t.IsFaulted)
                {
                    stateRef.HasResult = true;
                    stateRef.Result = t.Result;
                }

                // signal that the execution is done
                stateRef.Lock.Set();
            }
        }

        private sealed class StateRef<T>
        {
            /// <summary>
            /// Gets the lock used to wait for task's completion.
            /// </summary>
            public AutoResetEvent Lock { get; }

            /// <summary>
            /// Gets the exception that occured during task's execution, if any.
            /// </summary>
            public Exception Exception { get; set; }

            /// <summary>
            /// Gets the result returned by the task.
            /// </summary>
            public T Result { get; set; }

            /// <summary>
            /// Gets whether the task returned a result.
            /// </summary>
            public bool HasResult { get; set; } = false;

            public StateRef(AutoResetEvent @lock)
            {
                this.Lock = @lock;
            }
        }
    }

    public sealed class AsyncManualResetEvent
    {
        /// <summary>
        /// Gets whether this event has been signaled.
        /// </summary>
        public bool IsSet => this._resetTcs?.Task?.IsCompleted == true;

        private volatile TaskCompletionSource<bool> _resetTcs;

        /// <summary>
        /// Creates a new asynchronous synchronization event with initial state.
        /// </summary>
        /// <param name="initialState">Initial state of this event.</param>
        public AsyncManualResetEvent(bool initialState)
        {
            this._resetTcs = new TaskCompletionSource<bool>();
            if (initialState)
                this._resetTcs.TrySetResult(initialState);
        }

        // Spawn a threadpool thread instead of making a task
        // Maybe overkill, but I am less unsure of this than awaits and
        // potentially cross-scheduler interactions
        /// <summary>
        /// Asynchronously signal this event.
        /// </summary>
        /// <returns></returns>
        public Task SetAsync() => Task.Run(() => this._resetTcs.TrySetResult(true));

        /// <summary>
        /// Asynchronously wait for this event to be signaled.
        /// </summary>
        /// <returns></returns>
        public Task WaitAsync() => this._resetTcs.Task;

        /// <summary>
        /// Reset this event's signal state to unsignaled.
        /// </summary>
        public void Reset()
        {
            while (true)
            {
                var tcs = this._resetTcs;
                if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref this._resetTcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                    return;
            }
        }
    }

    public class AsyncEventArgs : EventArgs
    {
        public bool Handled { get; set; } = false;
    }

    public delegate Task AsyncEventHandler<in TSender, in TArgs>(TSender sender, TArgs e) where TArgs : AsyncEventArgs;

    public sealed class AsyncEvent<TSender, TArgs> : AsyncEvent
    where TArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the maximum alloted execution time for all handlers. Any event which causes the handler to time out 
        /// will raise a non-fatal <see cref="AsyncEventTimeoutException{TSender, TArgs}"/>.
        /// </summary>
        public TimeSpan MaximumExecutionTime { get; }

        private readonly object _lock = new object();
        private ImmutableArray<AsyncEventHandler<TSender, TArgs>> _handlers;
        private readonly AsyncEventExceptionHandler<TSender, TArgs> _exceptionHandler;

        /// <summary>
        /// Creates a new asynchronous event with specified name and exception handler.
        /// </summary>
        /// <param name="name">Name of this event.</param>
        /// <param name="maxExecutionTime">Maximum handler execution time. A value of <see cref="TimeSpan.Zero"/> means infinite.</param>
        /// <param name="exceptionHandler">Delegate which handles exceptions caused by this event.</param>
        public AsyncEvent(string name, TimeSpan maxExecutionTime, AsyncEventExceptionHandler<TSender, TArgs> exceptionHandler)
            : base(name)
        {
            this._handlers = ImmutableArray<AsyncEventHandler<TSender, TArgs>>.Empty;
            this._exceptionHandler = exceptionHandler;

            this.MaximumExecutionTime = maxExecutionTime;
        }

        /// <summary>
        /// Registers a new handler for this event.
        /// </summary>
        /// <param name="handler">Handler to register for this event.</param>
        public void Register(AsyncEventHandler<TSender, TArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (this._lock)
                this._handlers = this._handlers.Add(handler);
        }

        /// <summary>
        /// Unregisters an existing handler from this event.
        /// </summary>
        /// <param name="handler">Handler to unregister from the event.</param>
        public void Unregister(AsyncEventHandler<TSender, TArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (this._lock)
                this._handlers = this._handlers.Remove(handler);
        }

        /// <summary>
        /// Unregisters all existing handlers from this event.
        /// </summary>
        public void UnregisterAll()
        {
            this._handlers = ImmutableArray<AsyncEventHandler<TSender, TArgs>>.Empty;
        }

        /// <summary>
        /// <para>Raises this event by invoking all of its registered handlers, in order of registration.</para>
        /// <para>All exceptions throw during invocation will be handled by the event's registered exception handler.</para>
        /// </summary>
        /// <param name="sender">Object which raised this event.</param>
        /// <param name="e">Arguments for this event.</param>
        /// <param name="exceptionMode">Defines what to do with exceptions caught from handlers.</param>
        /// <returns></returns>
        public async Task InvokeAsync(TSender sender, TArgs e, AsyncEventExceptionMode exceptionMode = AsyncEventExceptionMode.Default)
        {
            var handlers = this._handlers;
            if (handlers.Length == 0)
                return;

            // Collect exceptions
            List<Exception> exceptions = null;
            if ((exceptionMode & AsyncEventExceptionMode.ThrowAll) != 0)
                exceptions = new List<Exception>(handlers.Length * 2 /* timeout + regular */);

            // If we have a timeout configured, start the timeout task
            var timeout = this.MaximumExecutionTime > TimeSpan.Zero ? Task.Delay(this.MaximumExecutionTime) : null;
            for (var i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i];
                try
                {
                    // Start the handler execution
                    var handlerTask = handler(sender, e);
                    if (handlerTask != null && timeout != null)
                    {
                        // If timeout is configured, wait for any task to finish
                        // If the timeout task finishes first, the handler is causing a timeout

                        var result = await Task.WhenAny(timeout, handlerTask).ConfigureAwait(false);
                        if (result == timeout)
                        {
                            timeout = null;
                            var timeoutEx = new AsyncEventTimeoutException<TSender, TArgs>(this, handler);

                            // Notify about the timeout and complete execution
                            if ((exceptionMode & AsyncEventExceptionMode.HandleNonFatal) == AsyncEventExceptionMode.HandleNonFatal)
                                this.HandleException(timeoutEx, handler, sender, e);

                            if ((exceptionMode & AsyncEventExceptionMode.ThrowNonFatal) == AsyncEventExceptionMode.ThrowNonFatal)
                                exceptions.Add(timeoutEx);
                        }
                    }

                    if (handlerTask != null)
                    {
                        // No timeout is configured, or timeout already expired, proceed as usual
                        await handlerTask.ConfigureAwait(false);
                    }

                    if (e.Handled)
                        break;
                }
                catch (Exception ex)
                {
                    e.Handled = false;

                    if ((exceptionMode & AsyncEventExceptionMode.HandleFatal) == AsyncEventExceptionMode.HandleFatal)
                        this.HandleException(ex, handler, sender, e);

                    if ((exceptionMode & AsyncEventExceptionMode.ThrowFatal) == AsyncEventExceptionMode.ThrowFatal)
                        exceptions.Add(ex);
                }
            }

            if ((exceptionMode & AsyncEventExceptionMode.ThrowAll) != 0 && exceptions.Count > 0)
                throw new AggregateException("Exceptions were thrown during execution of the event's handlers.", exceptions);
        }

        private void HandleException(Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs args)
        {
            if (this._exceptionHandler != null)
                this._exceptionHandler(this, ex, handler, sender, args);
        }
    }
}
