﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UtilityLib.Mvvms.Input
{
    /// <summary>
    /// A command that mirrors the functionality of <see cref="RelayCommand"/>, with the addition of
    /// accepting a <see cref="Func{TResult}"/> returning a <see cref="Task"/> as the execute
    /// action, and providing an <see cref="ExecutionTask"/> property that notifies changes when
    /// <see cref="ExecuteAsync"/> is invoked and when the returned <see cref="Task"/> completes.
    /// </summary>
    public sealed partial class AsyncRelayCommand : IAsyncRelayCommand, ICancellationAwareCommand
    {
        /// <summary>
        /// The cached <see cref="PropertyChangedEventArgs"/> for <see cref="ExecutionTask"/>.
        /// </summary>
        internal static readonly PropertyChangedEventArgs ExecutionTaskChangedEventArgs = new(nameof(ExecutionTask));

        /// <summary>
        /// The cached <see cref="PropertyChangedEventArgs"/> for <see cref="CanBeCanceled"/>.
        /// </summary>
        internal static readonly PropertyChangedEventArgs CanBeCanceledChangedEventArgs = new(nameof(CanBeCanceled));

        /// <summary>
        /// The cached <see cref="PropertyChangedEventArgs"/> for <see cref="IsCancellationRequested"/>.
        /// </summary>
        internal static readonly PropertyChangedEventArgs IsCancellationRequestedChangedEventArgs = new(nameof(IsCancellationRequested));

        /// <summary>
        /// The cached <see cref="PropertyChangedEventArgs"/> for <see cref="IsRunning"/>.
        /// </summary>
        internal static readonly PropertyChangedEventArgs IsRunningChangedEventArgs = new(nameof(IsRunning));

        /// <summary>
        /// The <see cref="Func{TResult}"/> to invoke when <see cref="Execute"/> is used.
        /// </summary>
        private readonly Func<Task>? execute;

        /// <summary>
        /// The cancelable <see cref="Func{T,TResult}"/> to invoke when <see cref="Execute"/> is used.
        /// </summary>
        /// <remarks>Only one between this and <see cref="execute"/> is not <see langword="null"/>.</remarks>
        private readonly Func<CancellationToken, Task>? cancelableExecute;

        /// <summary>
        /// The optional action to invoke when <see cref="CanExecute"/> is used.
        /// </summary>
        private readonly Func<bool>? canExecute;

        /// <summary>
        /// The options being set for the current command.
        /// </summary>
        private readonly AsyncRelayCommandOptions options;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> instance to use to cancel <see cref="cancelableExecute"/>.
        /// </summary>
        /// <remarks>This is only used when <see cref="cancelableExecute"/> is not <see langword="null"/>.</remarks>
        private CancellationTokenSource? cancellationTokenSource;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<Task> execute)
        {
            ArgumentNullException.ThrowIfNull(execute);

            this.execute = execute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="options">The options to use to configure the async command.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<Task> execute, AsyncRelayCommandOptions options)
        {
            ArgumentNullException.ThrowIfNull(execute);

            this.execute = execute;
            this.options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="cancelableExecute">The cancelable execution logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> is <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<CancellationToken, Task> cancelableExecute)
        {
            ArgumentNullException.ThrowIfNull(cancelableExecute);

            this.cancelableExecute = cancelableExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="cancelableExecute">The cancelable execution logic.</param>
        /// <param name="options">The options to use to configure the async command.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> is <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<CancellationToken, Task> cancelableExecute, AsyncRelayCommandOptions options)
        {
            ArgumentNullException.ThrowIfNull(cancelableExecute);

            this.cancelableExecute = cancelableExecute;
            this.options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="options">The options to use to configure the async command.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute, AsyncRelayCommandOptions options)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.execute = execute;
            this.canExecute = canExecute;
            this.options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="cancelableExecute">The cancelable execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<CancellationToken, Task> cancelableExecute, Func<bool> canExecute)
        {
            ArgumentNullException.ThrowIfNull(cancelableExecute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.cancelableExecute = cancelableExecute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="cancelableExecute">The cancelable execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="options">The options to use to configure the async command.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<CancellationToken, Task> cancelableExecute, Func<bool> canExecute, AsyncRelayCommandOptions options)
        {
            ArgumentNullException.ThrowIfNull(cancelableExecute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.cancelableExecute = cancelableExecute;
            this.canExecute = canExecute;
            this.options = options;
        }

        private Task? executionTask;

        /// <inheritdoc/>
        public Task? ExecutionTask
        {
            get => this.executionTask;
            private set
            {
                if (ReferenceEquals(this.executionTask, value))
                {
                    return;
                }

                this.executionTask = value;

                PropertyChanged?.Invoke(this, ExecutionTaskChangedEventArgs);
                PropertyChanged?.Invoke(this, IsRunningChangedEventArgs);

                bool isAlreadyCompletedOrNull = value?.IsCompleted ?? true;

                if (this.cancellationTokenSource is not null)
                {
                    PropertyChanged?.Invoke(this, CanBeCanceledChangedEventArgs);
                    PropertyChanged?.Invoke(this, IsCancellationRequestedChangedEventArgs);
                }

                // The branch is on a condition evaluated before raising the events above if
                // needed, to avoid race conditions with a task completing right after them.
                if (isAlreadyCompletedOrNull)
                {
                    return;
                }

                static async void MonitorTask(AsyncRelayCommand @this, Task task)
                {
                    //await task.GetAwaitableWithoutEndValidation();

                    if (ReferenceEquals(@this.executionTask, task))
                    {
                        @this.PropertyChanged?.Invoke(@this, ExecutionTaskChangedEventArgs);
                        @this.PropertyChanged?.Invoke(@this, IsRunningChangedEventArgs);

                        if (@this.cancellationTokenSource is not null)
                        {
                            @this.PropertyChanged?.Invoke(@this, CanBeCanceledChangedEventArgs);
                        }

                        if ((@this.options & AsyncRelayCommandOptions.AllowConcurrentExecutions) == 0)
                        {
                            @this.CanExecuteChanged?.Invoke(@this, EventArgs.Empty);
                        }
                    }
                }

                MonitorTask(this, value!);
            }
        }

        /// <inheritdoc/>
        public bool CanBeCanceled => IsRunning && this.cancellationTokenSource is { IsCancellationRequested: false };

        /// <inheritdoc/>
        public bool IsCancellationRequested => this.cancellationTokenSource is { IsCancellationRequested: true };

        /// <inheritdoc/>
        public bool IsRunning => ExecutionTask is { IsCompleted: false };

        /// <inheritdoc/>
        bool ICancellationAwareCommand.IsCancellationSupported => this.execute is null;

        /// <inheritdoc/>
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object? parameter)
        {
            bool canExecute = this.canExecute?.Invoke() != false;

            return canExecute && ((this.options & AsyncRelayCommandOptions.AllowConcurrentExecutions) != 0 || ExecutionTask is not { IsCompleted: false });
        }

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            Task executionTask = ExecuteAsync(parameter);

            // If exceptions shouldn't flow to the task scheduler, await the resulting task. This is
            // delegated to a separate method to keep this one more compact in case the option is set.
            if ((this.options & AsyncRelayCommandOptions.FlowExceptionsToTaskScheduler) == 0)
            {
                AwaitAndThrowIfFailed(executionTask);
            }
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(object? parameter)
        {
            Task executionTask;

            if (this.execute is not null)
            {
                // Non cancelable command delegate
                executionTask = ExecutionTask = this.execute();
            }
            else
            {
                // Cancel the previous operation, if one is pending
                this.cancellationTokenSource?.Cancel();

                CancellationTokenSource cancellationTokenSource = this.cancellationTokenSource = new();

                // Invoke the cancelable command delegate with a new linked token
                executionTask = ExecutionTask = this.cancelableExecute!(cancellationTokenSource.Token);
            }

            // If concurrent executions are disabled, notify the can execute change as well
            if ((this.options & AsyncRelayCommandOptions.AllowConcurrentExecutions) == 0)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            return executionTask;
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            if (this.cancellationTokenSource is CancellationTokenSource { IsCancellationRequested: false } cancellationTokenSource)
            {
                cancellationTokenSource.Cancel();

                PropertyChanged?.Invoke(this, CanBeCanceledChangedEventArgs);
                PropertyChanged?.Invoke(this, IsCancellationRequestedChangedEventArgs);
            }
        }

        /// <summary>
        /// Awaits an input <see cref="Task"/> and throws an exception on the calling context, if the task fails.
        /// </summary>
        /// <param name="executionTask">The input <see cref="Task"/> instance to await.</param>
        internal static async void AwaitAndThrowIfFailed(Task executionTask)
        {
            // Note: this method is purposefully an async void method awaiting the input task. This is done so that
            // if an async relay command is invoked synchronously (ie. when Execute is called, eg. from a binding),
            // exceptions in the wrapped delegate will not be ignored or just become visible through the ExecutionTask
            // property, but will be rethrown in the original synchronization context by default. This makes the behavior
            // more consistent with how normal commands work (where exceptions are also just normally propagated to the
            // caller context), and avoids getting an app into an inconsistent state in case a method faults without
            // other components being notified. It is also possible to not await this task and to instead ignore exceptions
            // and then inspect them manually from the ExecutionTask property, by constructing an async command instance
            // using the AsyncRelayCommandOptions.FlowExceptionsToTaskScheduler option. That will cause this call to
            // be skipped, and exceptions will just either normally be available through that property, or will otherwise
            // flow to the static TaskScheduler.UnobservedTaskException event if otherwise unobserved (eg. for logging).
            await executionTask;
        }
    }

    /// <summary>
    /// A generic command that provides a more specific version of <see cref="AsyncRelayCommand"/>.
    /// </summary>
    /// <typeparam name="T">The type of parameter being passed as input to the callbacks.</typeparam>
    public sealed partial class AsyncRelayCommand<T> : IAsyncRelayCommand<T>, ICancellationAwareCommand
    {
        /// <summary>
        /// The <see cref="Func{TResult}"/> to invoke when <see cref="Execute(T)"/> is used.
        /// </summary>
        private readonly Func<T?, Task>? execute;

        /// <summary>
        /// The cancelable <see cref="Func{T1,T2,TResult}"/> to invoke when <see cref="Execute(object?)"/> is used.
        /// </summary>
        private readonly Func<T?, CancellationToken, Task>? cancelableExecute;

        /// <summary>
        /// The optional action to invoke when <see cref="CanExecute(T)"/> is used.
        /// </summary>
        private readonly Predicate<T?>? canExecute;

        /// <summary>
        /// The options being set for the current command.
        /// </summary>
        private readonly AsyncRelayCommandOptions options;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> instance to use to cancel <see cref="cancelableExecute"/>.
        /// </summary>
        private CancellationTokenSource? cancellationTokenSource;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<T?, Task> execute)
        {
            ArgumentNullException.ThrowIfNull(execute);

            this.execute = execute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="options">The options to use to configure the async command.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<T?, Task> execute, AsyncRelayCommandOptions options)
        {
            ArgumentNullException.ThrowIfNull(execute);

            this.execute = execute;
            this.options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="cancelableExecute">The cancelable execution logic.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> is <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<T?, CancellationToken, Task> cancelableExecute)
        {
            ArgumentNullException.ThrowIfNull(cancelableExecute);

            this.cancelableExecute = cancelableExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="cancelableExecute">The cancelable execution logic.</param>
        /// <param name="options">The options to use to configure the async command.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> is <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<T?, CancellationToken, Task> cancelableExecute, AsyncRelayCommandOptions options)
        {
            ArgumentNullException.ThrowIfNull(cancelableExecute);

            this.cancelableExecute = cancelableExecute;
            this.options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<T?, Task> execute, Predicate<T?> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="options">The options to use to configure the async command.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<T?, Task> execute, Predicate<T?> canExecute, AsyncRelayCommandOptions options)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.execute = execute;
            this.canExecute = canExecute;
            this.options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="cancelableExecute">The cancelable execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<T?, CancellationToken, Task> cancelableExecute, Predicate<T?> canExecute)
        {
            ArgumentNullException.ThrowIfNull(cancelableExecute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.cancelableExecute = cancelableExecute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="cancelableExecute">The cancelable execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="options">The options to use to configure the async command.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="cancelableExecute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public AsyncRelayCommand(Func<T?, CancellationToken, Task> cancelableExecute, Predicate<T?> canExecute, AsyncRelayCommandOptions options)
        {
            ArgumentNullException.ThrowIfNull(cancelableExecute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.cancelableExecute = cancelableExecute;
            this.canExecute = canExecute;
            this.options = options;
        }

        private Task? executionTask;

        /// <inheritdoc/>
        public Task? ExecutionTask
        {
            get => this.executionTask;
            private set
            {
                if (ReferenceEquals(this.executionTask, value))
                {
                    return;
                }

                this.executionTask = value;

                PropertyChanged?.Invoke(this, AsyncRelayCommand.ExecutionTaskChangedEventArgs);
                PropertyChanged?.Invoke(this, AsyncRelayCommand.IsRunningChangedEventArgs);

                bool isAlreadyCompletedOrNull = value?.IsCompleted ?? true;

                if (this.cancellationTokenSource is not null)
                {
                    PropertyChanged?.Invoke(this, AsyncRelayCommand.CanBeCanceledChangedEventArgs);
                    PropertyChanged?.Invoke(this, AsyncRelayCommand.IsCancellationRequestedChangedEventArgs);
                }

                if (isAlreadyCompletedOrNull)
                {
                    return;
                }

                static async void MonitorTask(AsyncRelayCommand<T> @this, Task task)
                {
                    //await task.GetAwaitableWithoutEndValidation();

                    if (ReferenceEquals(@this.executionTask, task))
                    {
                        @this.PropertyChanged?.Invoke(@this, AsyncRelayCommand.ExecutionTaskChangedEventArgs);
                        @this.PropertyChanged?.Invoke(@this, AsyncRelayCommand.IsRunningChangedEventArgs);

                        if (@this.cancellationTokenSource is not null)
                        {
                            @this.PropertyChanged?.Invoke(@this, AsyncRelayCommand.CanBeCanceledChangedEventArgs);
                        }

                        if ((@this.options & AsyncRelayCommandOptions.AllowConcurrentExecutions) == 0)
                        {
                            @this.CanExecuteChanged?.Invoke(@this, EventArgs.Empty);
                        }
                    }
                }

                MonitorTask(this, value!);
            }
        }

        /// <inheritdoc/>
        public bool CanBeCanceled => IsRunning && this.cancellationTokenSource is { IsCancellationRequested: false };

        /// <inheritdoc/>
        public bool IsCancellationRequested => this.cancellationTokenSource is { IsCancellationRequested: true };

        /// <inheritdoc/>
        public bool IsRunning => ExecutionTask is { IsCompleted: false };

        /// <inheritdoc/>
        bool ICancellationAwareCommand.IsCancellationSupported => this.execute is null;

        /// <inheritdoc/>
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(T? parameter)
        {
            bool canExecute = this.canExecute?.Invoke(parameter) != false;

            return canExecute && ((this.options & AsyncRelayCommandOptions.AllowConcurrentExecutions) != 0 || ExecutionTask is not { IsCompleted: false });
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object? parameter)
        {
            // Special case, see RelayCommand<T>.CanExecute(object?) for more info
            if (parameter is null && default(T) is not null)
            {
                return false;
            }

            if (!RelayCommand<T>.TryGetCommandArgument(parameter, out T? result))
            {
                RelayCommand<T>.ThrowArgumentExceptionForInvalidCommandArgument(parameter);
            }

            return CanExecute(result);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(T? parameter)
        {
            Task executionTask = ExecuteAsync(parameter);

            if ((this.options & AsyncRelayCommandOptions.FlowExceptionsToTaskScheduler) == 0)
            {
                AsyncRelayCommand.AwaitAndThrowIfFailed(executionTask);
            }
        }

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            if (!RelayCommand<T>.TryGetCommandArgument(parameter, out T? result))
            {
                RelayCommand<T>.ThrowArgumentExceptionForInvalidCommandArgument(parameter);
            }

            Execute(result);
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(T? parameter)
        {
            Task executionTask;

            if (this.execute is not null)
            {
                // Non cancelable command delegate
                executionTask = ExecutionTask = this.execute(parameter);
            }
            else
            {
                // Cancel the previous operation, if one is pending
                this.cancellationTokenSource?.Cancel();

                CancellationTokenSource cancellationTokenSource = this.cancellationTokenSource = new();

                // Invoke the cancelable command delegate with a new linked token
                executionTask = ExecutionTask = this.cancelableExecute!(parameter, cancellationTokenSource.Token);
            }

            // If concurrent executions are disabled, notify the can execute change as well
            if ((this.options & AsyncRelayCommandOptions.AllowConcurrentExecutions) == 0)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            return executionTask;
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(object? parameter)
        {
            if (!RelayCommand<T>.TryGetCommandArgument(parameter, out T? result))
            {
                RelayCommand<T>.ThrowArgumentExceptionForInvalidCommandArgument(parameter);
            }

            return ExecuteAsync(result);
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            if (this.cancellationTokenSource is CancellationTokenSource { IsCancellationRequested: false } cancellationTokenSource)
            {
                cancellationTokenSource.Cancel();

                PropertyChanged?.Invoke(this, AsyncRelayCommand.CanBeCanceledChangedEventArgs);
                PropertyChanged?.Invoke(this, AsyncRelayCommand.IsCancellationRequestedChangedEventArgs);
            }
        }
    }

    public static class IAsyncRelayCommandExtensions
    {
        /// <summary>
        /// Creates an <see cref="ICommand"/> instance that can be used to cancel execution on the input command.
        /// The returned command will also notify when it can be executed based on the state of the wrapped command.
        /// </summary>
        /// <param name="command">The input <see cref="IAsyncRelayCommand"/> instance to create a cancellation command for.</param>
        /// <returns>An <see cref="ICommand"/> instance that can be used to monitor and signal cancellation for <paramref name="command"/>.</returns>
        /// <remarks>The returned instance is not guaranteed to be unique across multiple invocations with the same arguments.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="command"/> is <see langword="null"/>.</exception>
        public static ICommand CreateCancelCommand(this IAsyncRelayCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            // If the command is known not to ever allow cancellation, just reuse the same instance
            if (command is ICancellationAwareCommand { IsCancellationSupported: false })
            {
                return DisabledCommand.Instance;
            }

            // Create a new cancel command wrapping the input one
            return new CancelCommand(command);
        }
    }
}
