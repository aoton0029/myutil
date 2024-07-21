using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    static partial class Observable
    {
        public static IDisposable Disposable(Action delegatee) => new DelegatingDisposable(delegatee);

        public static IObserver<T> Observer<T>(Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null) => new DelegatingObserver<T>(onNext, onError, onCompleted);

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null) =>
            source == null
            ? throw new ArgumentNullException(nameof(source))
            : source.Subscribe(Observer(onNext, onError, onCompleted));
    }


    sealed class DelegatingDisposable(Action delegatee) : IDisposable
    {
        Action? delegatee = delegatee ?? throw new ArgumentNullException(nameof(delegatee));

        public void Dispose()
        {
            var delegatee = this.delegatee;
            if (delegatee == null || Interlocked.CompareExchange(ref this.delegatee, null, delegatee) != delegatee)
                return;
            delegatee();
        }
    }

    sealed class DelegatingObserver<T>(Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null) : IObserver<T>
    {
        readonly Action<T> onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));

        public void OnCompleted() => onCompleted?.Invoke();
        public void OnError(Exception error) => onError?.Invoke(error);
        public void OnNext(T value) => this.onNext(value);
    }

}
