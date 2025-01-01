using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityLib.Commands;

namespace UtilityLib.Mvvms.Input
{
    /// <summary>
    /// An interface expanding <see cref="IRelayCommand"/> to support asynchronous operations.
    /// </summary>
    public interface IAsyncRelayCommand : IRelayCommand, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the last scheduled <see cref="Task"/>, if available.
        /// This property notifies a change when the <see cref="Task"/> completes.
        /// </summary>
        Task? ExecutionTask { get; }

        /// <summary>
        /// Gets a value indicating whether a running operation for this command can currently be canceled.
        /// </summary>
        /// <remarks>
        /// The exact sequence of events that types implementing this interface should raise is as follows:
        /// <list type="bullet">
        /// <item>
        /// The command is initially not running: <see cref="IsRunning"/>, <see cref="CanBeCanceled"/>
        /// and <see cref="IsCancellationRequested"/> are <see langword="false"/>.
        /// </item>
        /// <item>
        /// The command starts running: <see cref="IsRunning"/> and <see cref="CanBeCanceled"/> switch to
        /// <see langword="true"/>. <see cref="IsCancellationRequested"/> is set to <see langword="false"/>.
        /// </item>
        /// <item>
        /// If the operation is canceled: <see cref="CanBeCanceled"/> switches to <see langword="false"/>
        /// and <see cref="IsCancellationRequested"/> switches to <see langword="true"/>.
        /// </item>
        /// <item>
        /// The operation completes: <see cref="IsRunning"/> and <see cref="CanBeCanceled"/> switch
        /// to <see langword="false"/>. The state of <see cref="IsCancellationRequested"/> is undefined.
        /// </item>
        /// </list>
        /// This only applies if the underlying logic for the command actually supports cancelation. If that is
        /// not the case, then <see cref="CanBeCanceled"/> and <see cref="IsCancellationRequested"/> will always remain
        /// <see langword="false"/> regardless of the current state of the command.
        /// </remarks>
        bool CanBeCanceled { get; }

        /// <summary>
        /// Gets a value indicating whether a cancelation request has been issued for the current operation.
        /// </summary>
        bool IsCancellationRequested { get; }

        /// <summary>
        /// Gets a value indicating whether the command currently has a pending operation being executed.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Provides a more specific version of <see cref="System.Windows.Input.ICommand.Execute"/>,
        /// also returning the <see cref="Task"/> representing the async operation being executed.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <returns>The <see cref="Task"/> representing the async operation being executed.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="parameter"/> is incompatible with the underlying command implementation.</exception>
        Task ExecuteAsync(object? parameter);

        /// <summary>
        /// Communicates a request for cancelation.
        /// </summary>
        /// <remarks>
        /// If the underlying command is not running, or if it does not support cancelation, this method will perform no action.
        /// Note that even with a successful cancelation, the completion of the current operation might not be immediate.
        /// </remarks>
        void Cancel();
    }

    /// <summary>
    /// A generic interface representing a more specific version of <see cref="IAsyncRelayCommand"/>.
    /// </summary>
    /// <typeparam name="T">The type used as argument for the interface methods.</typeparam>
    /// <remarks>This interface is needed to solve the diamond problem with base classes.</remarks>
    public interface IAsyncRelayCommand<in T> : IAsyncRelayCommand, IRelayCommand<T>
    {
        /// <summary>
        /// Provides a strongly-typed variant of <see cref="IAsyncRelayCommand.ExecuteAsync"/>.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <returns>The <see cref="Task"/> representing the async operation being executed.</returns>
        Task ExecuteAsync(T? parameter);
    }

    /// <summary>
    /// An interface expanding <see cref="ICommand"/> with the ability to raise
    /// the <see cref="ICommand.CanExecuteChanged"/> event externally.
    /// </summary>
    public interface IRelayCommand : ICommand
    {
        /// <summary>
        /// Notifies that the <see cref="ICommand.CanExecute"/> property has changed.
        /// </summary>
        void NotifyCanExecuteChanged();
    }

    /// <summary>
    /// A generic interface representing a more specific version of <see cref="IRelayCommand"/>.
    /// </summary>
    /// <typeparam name="T">The type used as argument for the interface methods.</typeparam>
    public interface IRelayCommand<in T> : IRelayCommand
    {
        /// <summary>
        /// Provides a strongly-typed variant of <see cref="ICommand.CanExecute(object)"/>.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <returns>Whether or not the current command can be executed.</returns>
        /// <remarks>Use this overload to avoid boxing, if <typeparamref name="T"/> is a value type.</remarks>
        bool CanExecute(T? parameter);

        /// <summary>
        /// Provides a strongly-typed variant of <see cref="ICommand.Execute(object)"/>.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <remarks>Use this overload to avoid boxing, if <typeparamref name="T"/> is a value type.</remarks>
        void Execute(T? parameter);
    }
}
