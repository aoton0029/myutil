using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    class Subject<T> : IObservable<T>, IObserver<T>
    {
        List<IObserver<T>>? observers;
        bool completed;
        Exception? error;

        bool HasObservers => (this.observers?.Count ?? 0) > 0;
        List<IObserver<T>> Observers => this.observers ??= [];

        bool IsMuted => this.completed || this.error != null;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            if (this.error != null)
            {
                observer.OnError(this.error);
                return Disposable.Empty;
            }

            if (this.completed)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }

            Observers.Add(observer);

            return Disposable.Create(() =>
            {
                var observers = Observers;

                for (var i = 0; i < observers.Count; i++)
                {
                    if (observers[i] == observer)
                    {
                        if (this.shouldDeleteObserver)
                            observers[i] = null!;
                        else
                            observers.RemoveAt(i);
                        break;
                    }
                }
            });
        }

        bool shouldDeleteObserver; // delete (null) or remove an observer?

        public void OnNext(T value)
        {
            if (!HasObservers)
                return;

            var observers = Observers;

            this.shouldDeleteObserver = true;

            try
            {

                for (var i = 0; i < observers.Count; i++)
                    observers[i].OnNext(value);
            }
            finally
            {
                this.shouldDeleteObserver = false;

                _ = observers.RemoveAll(o => o == null);
            }
        }

        public void OnError(Exception error) => OnFinality(ref this.error, error, (observer, err) => observer.OnError(err));

        public void OnCompleted() => OnFinality(ref this.completed, true, (observer, _) => observer.OnCompleted());

        void OnFinality<TState>(ref TState? state, TState value, Action<IObserver<T>, TState> action)
        {
            if (IsMuted)
                return;

            state = value;

            var observers = this.observers;
            this.observers = null;

            if (observers == null)
                return;

            foreach (var observer in observers)
                action(observer, value);
        }
    }

    public class Unsubscriber<T> : IDisposable
    {
        private List<IObserver<T>> _observers;
        private IObserver<T> _observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }
}
