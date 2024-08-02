using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UtilityLib.Interop;

namespace UtilityLib.Tasks.AsyncTasks
{
    public static class EventAsyncFactory
    {
        public static async Task<TEventArguments> FromAnyEvent<TDelegate, TEventArguments>(Func<Action<TEventArguments>, TDelegate> convert,
            Action<TDelegate> subscribe, Action<TDelegate> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        {
            _ = convert ?? throw new ArgumentNullException(nameof(convert));
            _ = subscribe ?? throw new ArgumentNullException(nameof(subscribe));
            _ = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));

            cancellationToken.ThrowIfCancellationRequested();
            var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<TEventArguments>();
            var subscription = convert(result => tcs.TrySetResult(result));
            try
            {
                using (cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
                {
                    subscribe(subscription);
                    return await tcs.Task.ConfigureAwait(continueOnCapturedContext: unsubscribeOnCapturedContext);
                }
            }
            finally
            {
                unsubscribe(subscription);
            }
        }

        public static Task<TEventArguments> FromAnyEvent<TDelegate, TEventArguments>(Func<Action<TEventArguments>, TDelegate> convert,
            Action<TDelegate> subscribe, Action<TDelegate> unsubscribe, CancellationToken cancellationToken)
        => FromAnyEvent(convert, subscribe, unsubscribe, cancellationToken, true);

        public static Task<TEventArguments> FromAnyEvent<TDelegate, TEventArguments>(Func<Action<TEventArguments>, TDelegate> convert,
            Action<TDelegate> subscribe, Action<TDelegate> unsubscribe)
        => FromAnyEvent(convert, subscribe, unsubscribe, CancellationToken.None, true);

        public static Task<EventArguments<object, EventArgs>> FromEvent(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<EventHandler, EventArguments<object, EventArgs>>(x => (sender, args) => x(CreateEventArguments(sender, args)), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        public static Task<EventArguments<object, EventArgs>> FromEvent(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe, CancellationToken cancellationToken)
        => FromEvent(subscribe, unsubscribe, cancellationToken, true);

        public static Task<EventArguments<object, EventArgs>> FromEvent(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe)
        => FromEvent(subscribe, unsubscribe, CancellationToken.None, true);

        public static Task<EventArguments<object, TEventArgs>> FromEvent<TEventArgs>(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<EventHandler<TEventArgs>, EventArguments<object, TEventArgs>>(x => (sender, args) => x(CreateEventArguments(sender, args)), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        public static Task<EventArguments<object, TEventArgs>> FromEvent<TEventArgs>(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe, CancellationToken cancellationToken)
        => FromEvent(subscribe, unsubscribe, cancellationToken, true);

        public static Task<EventArguments<object, TEventArgs>> FromEvent<TEventArgs>(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe)
        => FromEvent(subscribe, unsubscribe, CancellationToken.None, true);

        public static Task<EventArguments<object, TEventArgs>> FromEvent<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> convert, Action<TDelegate> subscribe, Action<TDelegate> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<TDelegate, EventArguments<object, TEventArgs>>(x => convert((sender, args) => x(CreateEventArguments(sender, args))), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        public static Task<EventArguments<object, TEventArgs>> FromEvent<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> convert, Action<TDelegate> subscribe, Action<TDelegate> unsubscribe, CancellationToken cancellationToken)
        => FromEvent(convert, subscribe, unsubscribe, cancellationToken, true);

        public static Task<EventArguments<object, TEventArgs>> FromEvent<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> convert, Action<TDelegate> subscribe, Action<TDelegate> unsubscribe)
        => FromEvent(convert, subscribe, unsubscribe, CancellationToken.None, true);

        public static Task<EventArguments<TSender, TEventArgs>> FromActionEvent<TSender, TEventArgs>(Action<Action<TSender, TEventArgs>> subscribe, Action<Action<TSender, TEventArgs>> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<Action<TSender, TEventArgs>, EventArguments<TSender, TEventArgs>>(x => (sender, args) => x(CreateEventArguments(sender, args)), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        public static Task<EventArguments<TSender, TEventArgs>> FromActionEvent<TSender, TEventArgs>(Action<Action<TSender, TEventArgs>> subscribe, Action<Action<TSender, TEventArgs>> unsubscribe, CancellationToken cancellationToken)
        => FromActionEvent(subscribe, unsubscribe, cancellationToken, true);

        public static Task<EventArguments<TSender, TEventArgs>> FromActionEvent<TSender, TEventArgs>(Action<Action<TSender, TEventArgs>> subscribe, Action<Action<TSender, TEventArgs>> unsubscribe)
        => FromActionEvent(subscribe, unsubscribe, CancellationToken.None, true);

        public static Task<TEventArgs> FromActionEvent<TEventArgs>(Action<Action<TEventArgs>> subscribe, Action<Action<TEventArgs>> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<Action<TEventArgs>, TEventArgs>(x => x, subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        public static Task<TEventArgs> FromActionEvent<TEventArgs>(Action<Action<TEventArgs>> subscribe, Action<Action<TEventArgs>> unsubscribe, CancellationToken cancellationToken)
        => FromActionEvent(subscribe, unsubscribe, cancellationToken, true);

        public static Task<TEventArgs> FromActionEvent<TEventArgs>(Action<Action<TEventArgs>> subscribe, Action<Action<TEventArgs>> unsubscribe)
        => FromActionEvent(subscribe, unsubscribe, CancellationToken.None, true);

        public static Task FromActionEvent(Action<Action> subscribe, Action<Action> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<Action, object?>(x => () => x(null), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        public static Task FromActionEvent(Action<Action> subscribe, Action<Action> unsubscribe, CancellationToken cancellationToken)
            => FromActionEvent(subscribe, unsubscribe, cancellationToken, true);

        public static Task FromActionEvent(Action<Action> subscribe, Action<Action> unsubscribe)
            => FromActionEvent(subscribe, unsubscribe, CancellationToken.None, true);

        private static EventArguments<TSender, TEventArgs> CreateEventArguments<TSender, TEventArgs>(TSender sender, TEventArgs eventArgs)
        => new EventArguments<TSender, TEventArgs> { Sender = sender, EventArgs = eventArgs };
    }
}
