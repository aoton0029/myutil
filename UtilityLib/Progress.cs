using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class Progress<T> : IProgress<T>
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly Action<T>? _handler;
        private readonly SendOrPostCallback _invokeHandlers;

        public Progress()
        {
            _synchronizationContext = SynchronizationContext.Current ?? ProgressStatics.DefaultContext;
            Debug.Assert(_synchronizationContext != null);
            _invokeHandlers = new SendOrPostCallback(InvokeHandlers);
        }

        public Progress(Action<T> handler) : this()
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public event Action<object?, T>? ProgressChanged;

        protected virtual void OnReport(T value)
        {
            Action<T>? handler = _handler;
            Action<object?, T>? changedEvent = ProgressChanged;
            if (handler != null || changedEvent != null)
            {
                _synchronizationContext.Post(_invokeHandlers, value);
            }
        }

        void IProgress<T>.Report(T value) { OnReport(value); }

        private void InvokeHandlers(object? state)
        {
            T value = (T)state!;

            Action<T>? handler = _handler;
            Action<object?, T>? changedEvent = ProgressChanged;

            handler?.Invoke(value);
            changedEvent?.Invoke(this, value);
        }
    }

    internal static class ProgressStatics
    {
        internal static readonly SynchronizationContext DefaultContext = new SynchronizationContext();
    }

}
