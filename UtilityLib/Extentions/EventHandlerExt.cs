//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace UtilityLib.Extentions
//{
//    public static class EventHandlerExt
//    {
//        public static Task InvokeAsync<T>(this EventHandler<T>? eventHandler, object sender, T eventArgs) where T : DeferredEventArgs
//        {
//            return InvokeAsync(eventHandler, sender, eventArgs, CancellationToken.None);
//        }

//        public static Task InvokeAsync<T>(this EventHandler<T>? eventHandler, object sender, T eventArgs, CancellationToken cancellationToken) where T : DeferredEventArgs
//        {
//            if (eventHandler == null)
//            {
//                return Task.CompletedTask;
//            }

//            Task[]? tasks = eventHandler.GetInvocationList()
//                .OfType<EventHandler<T>>()
//                .Select(invocationDelegate =>
//                {
//                    cancellationToken.ThrowIfCancellationRequested();

//                    invocationDelegate(sender, eventArgs);

//                    EventDeferral? deferral = eventArgs.GetCurrentDeferralAndReset();

//                    return deferral?.WaitForCompletion(cancellationToken) ?? Task.CompletedTask;
//                })
//                .ToArray();

//            return Task.WhenAll(tasks);
//        }
//    }

//    public class EventDeferral : IDisposable
//    {
//        private readonly TaskCompletionSource<object?> taskCompletionSource = new();

//        internal EventDeferral()
//        {

//        }

//        public void Complete() => this.taskCompletionSource.TrySetResult(null);

//        public async Task WaitForCompletion(CancellationToken cancellationToken)
//        {
//            using (cancellationToken.Register(() => this.taskCompletionSource.TrySetCanceled()))
//            {
//                _ = await this.taskCompletionSource.Task;
//            }
//        }

//        /// <inheritdoc/>
//        public void Dispose()
//        {
//            Complete();
//        }
//    }

//    public class DeferredEventArgs : EventArgs
//    {
//        public static new DeferredEventArgs Empty => new();

//        private readonly object eventDeferralLock = new();

//        private EventDeferral? eventDeferral;

//        public EventDeferral GetDeferral()
//        {
//            lock (this.eventDeferralLock)
//            {
//                return this.eventDeferral ??= new EventDeferral();
//            }
//        }

//        public EventDeferral? GetCurrentDeferralAndReset()
//        {
//            lock (this.eventDeferralLock)
//            {
//                EventDeferral? eventDeferral = this.eventDeferral;

//                this.eventDeferral = null;

//                return eventDeferral;
//            }
//        }
//    }

//    public class DeferredCancelEventArgs : DeferredEventArgs
//    {
//        public bool Cancel { get; set; }
//    }

//    public static class NativeEventWaiter
//    {
//        public static void WaitForEventLoop(string eventName, Action callback, Dispatcher dispatcher, CancellationToken cancel)
//        {
//            new Thread(() =>
//            {
//                var eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
//                while (true)
//                {
//                    if (WaitHandle.WaitAny(new WaitHandle[] { cancel.WaitHandle, eventHandle }) == 1)
//                    {
//                        dispatcher.BeginInvoke(callback);
//                    }
//                    else
//                    {
//                        return;
//                    }
//                }
//            }).Start();
//        }
//    }
//}
