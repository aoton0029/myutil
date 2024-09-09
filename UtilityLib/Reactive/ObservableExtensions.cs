using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    public static partial class ObservableExtensions
    {
        public static IObserver<T> Observer<T>(Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null) => new DelegatingObserver<T>(onNext, onError, onCompleted);

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null) =>
            source == null
            ? throw new ArgumentNullException(nameof(source))
            : source.Subscribe(Observer(onNext, onError, onCompleted));


        // Where: フィルタリングする拡張メソッド
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            return new AnonymousObservable<T>(observer =>
            {
                return source.Subscribe(new AnonymousObserver<T>(
                    onNext: value =>
                    {
                        if (predicate(value))
                        {
                            observer.OnNext(value);
                        }
                    },
                    onError: observer.OnError,
                    onCompleted: observer.OnCompleted
                ));
            });
        }

        // Select: 変換する拡張メソッド
        public static IObservable<TResult> Select<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TResult> selector)
        {
            return new AnonymousObservable<TResult>(observer =>
            {
                return source.Subscribe(new AnonymousObserver<TSource>(
                    onNext: value => observer.OnNext(selector(value)),
                    onError: observer.OnError,
                    onCompleted: observer.OnCompleted
                ));
            });
        }



        public static IObservable<T> Repeat<T>(this IObservable<T> source, int repeatCount)
        {
            return new AnonymousObservable<T>(observer =>
            {
                int count = 0;
                IDisposable subscription = null;

                Action resubscribe = null;
                resubscribe = () =>
                {
                    subscription = source.Subscribe(
                        value => observer.OnNext(value),
                        error => observer.OnError(error),
                        () =>
                        {
                            if (++count < repeatCount)
                            {
                                resubscribe();  // 繰り返し
                            }
                            else
                            {
                                observer.OnCompleted();
                            }
                        });
                };

                resubscribe();
                return subscription;
            });
        }

        public static IObservable<T> Delay<T>(this IObservable<T> source, TimeSpan delay)
        {
            return new AnonymousObservable<T>(observer =>
            {
                return source.Subscribe(
                    async value =>
                    {
                        await Task.Delay(delay);
                        observer.OnNext(value);
                    },
                    error => observer.OnError(error),
                    () => observer.OnCompleted()
                );
            });
        }

        public static IObservable<T> Do<T>(this IObservable<T> source, Action<T> onNextAction)
        {
            return new AnonymousObservable<T>(observer =>
            {
                return source.Subscribe(
                    value =>
                    {
                        onNextAction(value); // 副作用
                        observer.OnNext(value); // 値はそのまま渡す
                    },
                    error => observer.OnError(error),
                    () => observer.OnCompleted()
                );
            });
        }

        public static IObservable<T> Catch<T>(this IObservable<T> source, Func<Exception, IObservable<T>> errorHandler)
        {
            return new AnonymousObservable<T>(observer =>
            {
                return source.Subscribe(
                    value => observer.OnNext(value),
                    error =>
                    {
                        var recovery = errorHandler(error);
                        recovery.Subscribe(observer);
                    },
                    () => observer.OnCompleted()
                );
            });
        }

        public static IObservable<long> Interval(TimeSpan period)
        {
            return new AnonymousObservable<long>(observer =>
            {
                var timer = new System.Threading.Timer(_ =>
                {
                    try
                    {
                        observer.OnNext(DateTimeOffset.Now.ToUnixTimeMilliseconds());
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }, null, TimeSpan.Zero, period);

                return Disposable.Create(() =>
                {
                    timer.Dispose();
                    observer.OnCompleted();
                });
            });
        }

        public static IObservable<string> ReadFromFileAsync(this IObservable<Unit> trigger, string filePath)
        {
            return new AnonymousObservable<string>(observer =>
            {
                return trigger.Subscribe(async _ =>
                {
                    try
                    {
                        string content = await File.ReadAllTextAsync(filePath); // 非同期でファイルを読み込み
                        observer.OnNext(content); // 読み込んだデータを発行
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                error => observer.OnError(error),
                () => observer.OnCompleted());
            });
        }

        public static IObservable<string> WriteToFileAsync(this IObservable<string> source, string filePath)
        {
            return new AnonymousObservable<string>(observer =>
            {
                return source.Subscribe(async content =>
                {
                    try
                    {
                        await File.WriteAllTextAsync(filePath, content); // 非同期でファイルに書き込み
                        observer.OnNext(content); // 成功したデータを発行
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                error => observer.OnError(error),
                () => observer.OnCompleted());
            });
        }

        public static IObservable<TResult> FromAsync<TResult>(Func<Task<TResult>> asyncFunction)
        {
            return new AnonymousObservable<TResult>(observer =>
            {
                var task = asyncFunction();

                task.ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        observer.OnError(t.Exception.InnerException);
                    }
                    else if (t.IsCanceled)
                    {
                        observer.OnError(new TaskCanceledException(t));
                    }
                    else
                    {
                        observer.OnNext(t.Result);
                        observer.OnCompleted();
                    }
                });

                return Disposable.Empty; // 購読解除用のディスポーザブルを返す
            });
        }

        public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
        {
            return new AnonymousObservable<TResult>(observer =>
            {
                var subscriptions = new List<IDisposable>();

                var outerSubscription = source.Subscribe(
                    onNext: value =>
                    {
                        var innerObservable = selector(value);
                        var innerSubscription = innerObservable.Subscribe(
                            onNext: observer.OnNext,
                            onError: observer.OnError,
                            onCompleted: observer.OnCompleted
                        );
                        subscriptions.Add(innerSubscription);
                    },
                    onError: observer.OnError,
                    onCompleted: observer.OnCompleted
                );

                subscriptions.Add(outerSubscription);
                return new CompositeDisposable(subscriptions);
            });
        }

        public static IObservable<TResult> Join<TLeft, TRight, TResult>(this IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, TRight, TResult> resultSelector)
        {
            return new AnonymousObservable<TResult>(observer =>
            {
                TLeft leftValue = default;
                TRight rightValue = default;
                bool leftHasValue = false;
                bool rightHasValue = false;

                var leftSubscription = left.Subscribe(
                    onNext: lv =>
                    {
                        leftValue = lv;
                        leftHasValue = true;
                        if (rightHasValue)
                        {
                            observer.OnNext(resultSelector(leftValue, rightValue));
                        }
                    },
                    onError: observer.OnError,
                    onCompleted: observer.OnCompleted
                );

                var rightSubscription = right.Subscribe(
                    onNext: rv =>
                    {
                        rightValue = rv;
                        rightHasValue = true;
                        if (leftHasValue)
                        {
                            observer.OnNext(resultSelector(leftValue, rightValue));
                        }
                    },
                    onError: observer.OnError,
                    onCompleted: observer.OnCompleted
                );

                return new CompositeDisposable(leftSubscription, rightSubscription);
            });
        }

        public static IObservable<T> Merge<T>(this IObservable<T> first, IObservable<T> second)
        {
            return new AnonymousObservable<T>(observer =>
            {
                var subscriptions = new List<IDisposable>();

                subscriptions.Add(first.Subscribe(observer));
                subscriptions.Add(second.Subscribe(observer));

                return new CompositeDisposable(subscriptions);
            });
        }

        public static IObservable<T> Using<T, TResource>(Func<TResource> resourceFactory, Func<TResource, IObservable<T>> observableFactory) where TResource : IDisposable
        {
            return new AnonymousObservable<T>(observer =>
            {
                var resource = resourceFactory();
                var observable = observableFactory(resource);
                var subscription = observable.Subscribe(observer);

                return new CompositeDisposable(subscription, resource);
            });
        }

        public static IObservable<TEventArgs> FromEvent<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler)
        {
            return new AnonymousObservable<TEventArgs>(observer =>
            {
                EventHandler<TEventArgs> handler = (sender, args) => observer.OnNext(args);
                addHandler(handler);

                return Disposable.Create(() => removeHandler(handler));
            });
        }

        public static IObservable<T> Concat<T>(this IObservable<T> first, IObservable<T> second)
        {
            return new AnonymousObservable<T>(observer =>
            {
                var subscriptions = new List<IDisposable>();

                var firstCompleted = false;

                var firstSubscription = first.Subscribe(
                    onNext: observer.OnNext,
                    onError: observer.OnError,
                    onCompleted: () =>
                    {
                        firstCompleted = true;
                        var secondSubscription = second.Subscribe(observer);
                        subscriptions.Add(secondSubscription);
                    });

                subscriptions.Add(firstSubscription);

                return new CompositeDisposable(subscriptions);
            });
        }

        public static IConnectableObservable<T> Publish<T>(this IObservable<T> source)
        {
            var observers = new List<IObserver<T>>();
            bool connected = false;

            return new AnonymousConnectableObservable<T>(observer =>
            {
                if (!connected)
                {
                    source.Subscribe(
                        onNext: value =>
                        {
                            foreach (var o in observers)
                            {
                                o.OnNext(value);
                            }
                        },
                        onError: ex =>
                        {
                            foreach (var o in observers)
                            {
                                o.OnError(ex);
                            }
                        },
                        onCompleted: () =>
                        {
                            foreach (var o in observers)
                            {
                                o.OnCompleted();
                            }
                        });
                    connected = true;
                }
                observers.Add(observer);
                return Disposable.Create(() => observers.Remove(observer));
            });
        }

        public static IObservable<(T value, DateTime timestamp)> Timestamp<T>(this IObservable<T> source)
        {
            return new AnonymousObservable<(T value, DateTime timestamp)>(observer =>
            {
                return source.Subscribe(
                    onNext: value => observer.OnNext((value, DateTime.Now)),
                    onError: observer.OnError,
                    onCompleted: observer.OnCompleted
                );
            });
        }


    }



}
