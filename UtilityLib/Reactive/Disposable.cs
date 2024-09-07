using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    
    sealed class Disposable : IDisposable
    {
        public static readonly IDisposable Empty = new EmptyDisposable();

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
                // 何もしない
            }
        }

        public static IDisposable Create(Action disposeAction)
        {
            return new ActionDisposable(disposeAction);
        }

        private class ActionDisposable : IDisposable
        {
            private readonly Action _disposeAction;
            private bool _isDisposed = false;

            public ActionDisposable(Action disposeAction)
            {
                _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
            }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _disposeAction(); // 指定されたアクションを実行
                    _isDisposed = true;
                }
            }
        }

        public void Dispose()
        {
            // デフォルトの Dispose 実装
        }
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
