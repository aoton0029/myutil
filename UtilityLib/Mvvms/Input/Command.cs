using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UtilityLib.Mvvms.Input
{
    /// <summary>
    /// A <see cref="ICommand"/> implementation wrapping <see cref="IAsyncRelayCommand"/> to support cancellation.
    /// </summary>
    internal sealed partial class CancelCommand : ICommand
    {
        /// <summary>
        /// The wrapped <see cref="IAsyncRelayCommand"/> instance.
        /// </summary>
        private readonly IAsyncRelayCommand command;

        /// <summary>
        /// Creates a new <see cref="CancelCommand"/> instance.
        /// </summary>
        /// <param name="command">The <see cref="IAsyncRelayCommand"/> instance to wrap.</param>
        public CancelCommand(IAsyncRelayCommand command)
        {
            this.command = command;

            this.command.PropertyChanged += OnPropertyChanged;
        }

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc/>
        public bool CanExecute(object? parameter)
        {
            return this.command.CanBeCanceled;
        }

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            this.command.Cancel();
        }

        /// <inheritdoc cref="PropertyChangedEventHandler"/>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is null or nameof(IAsyncRelayCommand.CanBeCanceled))
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// A reusable <see cref="ICommand"/> instance that is always disabled.
    /// </summary>
    internal sealed partial class DisabledCommand : ICommand
    {
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Gets a shared, reusable <see cref="DisabledCommand"/> instance.
        /// </summary>
        /// <remarks>
        /// This instance can safely be used across multiple objects without having
        /// to worry about this static keeping others alive, as the event uses a
        /// custom accessor that just discards items (as the event is known to never
        /// be raised). As such, this instance will never act as root for other objects.
        /// </remarks>
        public static DisabledCommand Instance { get; } = new();

        /// <inheritdoc/>
        public bool CanExecute(object? parameter)
        {
            return false;
        }

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
        }
    }

    /// <summary>
    /// An interface for commands that know whether they support cancellation or not.
    /// </summary>
    internal interface ICancellationAwareCommand
    {
        /// <summary>
        /// Gets whether or not the current command supports cancellation.
        /// </summary>
        bool IsCancellationSupported { get; }
    }
}
