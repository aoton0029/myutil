using System;
using System.Threading;
using System.Threading.Tasks;

public static class ObservableExtensions
{
    // ObserveOn: 通知を特定のスケジューラ（スレッド）上で行う
    public static IObservable<T> ObserveOn<T>(this IObservable<T> source, SynchronizationContext context)
    {
        return new AnonymousObservable<T>(observer =>
        {
            return source.Subscribe(
                onNext: value =>
                {
                    context.Post(_ => observer.OnNext(value), null);
                },
                onError: ex =>
                {
                    context.Post(_ => observer.OnError(ex), null);
                },
                onCompleted: () =>
                {
                    context.Post(_ => observer.OnCompleted(), null);
                });
        });
    }

    // 別スレッドで実行するObserveOn（タスクで処理）
    public static IObservable<T> ObserveOn<T>(this IObservable<T> source, TaskScheduler scheduler)
    {
        return new AnonymousObservable<T>(observer =>
        {
            return source.Subscribe(
                onNext: value =>
                {
                    Task.Factory.StartNew(() => observer.OnNext(value), CancellationToken.None, TaskCreationOptions.None, scheduler);
                },
                onError: ex =>
                {
                    Task.Factory.StartNew(() => observer.OnError(ex), CancellationToken.None, TaskCreationOptions.None, scheduler);
                },
                onCompleted: () =>
                {
                    Task.Factory.StartNew(() => observer.OnCompleted(), CancellationToken.None, TaskCreationOptions.None, scheduler);
                });
        });
    }
}


using System;
using System.Threading;

public static class ObservableExtensions
{
    public static IObservable<T> RefCount<T>(this IConnectableObservable<T> source)
    {
        int refCount = 0;
        IDisposable connection = null;
        object gate = new object();

        return new AnonymousObservable<T>(observer =>
        {
            lock (gate)
            {
                refCount++;
                if (refCount == 1)
                {
                    connection = source.Connect();
                }
            }

            var subscription = source.Subscribe(observer);

            return Disposable.Create(() =>
            {
                subscription.Dispose();

                lock (gate)
                {
                    refCount--;
                    if (refCount == 0)
                    {
                        connection.Dispose();
                        connection = null;
                    }
                }
            });
        });
    }
}


using System;
using System.Threading;
using System.Threading.Tasks;

public static class ObservableExtensions
{
    // ObserveOn: 通知を特定のスケジューラ（スレッド）上で行う
    public static IObservable<T> ObserveOn<T>(this IObservable<T> source, SynchronizationContext context)
    {
        return new AnonymousObservable<T>(observer =>
        {
            return source.Subscribe(
                onNext: value =>
                {
                    context.Post(_ => observer.OnNext(value), null);
                },
                onError: ex =>
                {
                    context.Post(_ => observer.OnError(ex), null);
                },
                onCompleted: () =>
                {
                    context.Post(_ => observer.OnCompleted(), null);
                });
        });
    }

    // 別スレッドで実行するObserveOn（タスクで処理）
    public static IObservable<T> ObserveOn<T>(this IObservable<T> source, TaskScheduler scheduler)
    {
        return new AnonymousObservable<T>(observer =>
        {
            return source.Subscribe(
                onNext: value =>
                {
                    Task.Factory.StartNew(() => observer.OnNext(value), CancellationToken.None, TaskCreationOptions.None, scheduler);
                },
                onError: ex =>
                {
                    Task.Factory.StartNew(() => observer.OnError(ex), CancellationToken.None, TaskCreationOptions.None, scheduler);
                },
                onCompleted: () =>
                {
                    Task.Factory.StartNew(() => observer.OnCompleted(), CancellationToken.None, TaskCreationOptions.None, scheduler);
                });
        });
    }
}


using System;
using System.Collections.Generic;

public class ConnectableObservable<T> : IConnectableObservable<T>
{
    private readonly IObservable<T> _source;
    private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();
    private IDisposable _connection;
    private bool _connected = false;

    public ConnectableObservable(IObservable<T> source)
    {
        _source = source;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        return Disposable.Create(() => _observers.Remove(observer));
    }

    public IDisposable Connect()
    {
        if (!_connected)
        {
            _connection = _source.Subscribe(
                onNext: value =>
                {
                    foreach (var observer in _observers)
                    {
                        observer.OnNext(value);
                    }
                },
                onError: ex =>
                {
                    foreach (var observer in _observers)
                    {
                        observer.OnError(ex);
                    }
                },
                onCompleted: () =>
                {
                    foreach (var observer in _observers)
                    {
                        observer.OnCompleted();
                    }
                });

            _connected = true;
        }

        return Disposable.Create(() =>
        {
            _connection.Dispose();
            _connected = false;
        });
    }
}

