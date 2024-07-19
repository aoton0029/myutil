using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class Delegates
    {
        public static IDisposable Disposable(Action delegatee) => new DelegatingDisposable(delegatee);

        public static IObserver<T> Observer<T>(Action<T> onNext,
                                               Action<T> onBack,
                                               Action<Exception>? onError = null,
                                               Action? onCompleted = null,
                                               Action? onCanceled = null,
                                               Action? onTerminated = null) => new DelegatingObserver<T>(onNext, onBack, onError, onCompleted, onCanceled, onTerminated);
    }

    sealed class DelegatingDisposable(Action delegatee) : IDisposable
    {
        Action? delegatee = delegatee ?? throw new ArgumentNullException(nameof(delegatee));

        public void Dispose()
        {
            var delegatee = this.delegatee;
            if (delegatee == null || Interlocked.CompareExchange(ref this.delegatee, null, delegatee) != delegatee)
            {
                return;
            }
            delegatee();
        }
    }

    sealed class DelegatingObserver<T>(Action<T> onNext, 
                                       Action<T> onBack, 
                                       Action<Exception>? onError = null, 
                                       Action? onCompleted = null, 
                                       Action? onCanceled = null, 
                                       Action? onTerminated = null) : IObserver<T>
    {
        public void OnNext(T value) => onNext(value);
        public void OnBack(T value) => onBack(value);
        public void OnCancel() => onCanceled?.Invoke();
        public void OnTerminated() => onTerminated?.Invoke();
        public void OnCompleted() => onCompleted?.Invoke();
        public void OnError(Exception error) => onError?.Invoke(error);
    }

}
