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


        public static IObservable<T> MyWhere<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            return new WhereObservable<T>(observer =>
            {
                return source.Subscribe(
                    new WhereObserver<T>(
                        value =>
                        {
                            //--- ここはOnNext実行時に呼び出される
                            if (predicate(value))
                            {
                                observer.OnNext(value);
                            }
                        },
                        observer.OnError,       //--- OnErrorと
                        observer.OnCompleted)); //--- OnCompletedは何もせず素通し
            });
        }


        public static IObservable<TResult> MySelect<T, TResult>(this IObservable<T> source, Func<T, TResult> selector)
        {
            //Observableは値を送信する側なので、変換された値を送信するためTResult型
            return new SelectObservable<TResult>(observer =>
            {
                return source.Subscribe(
                    //Observerは上からOnNextされた値を受け取るのでT型
                    new SelectObserver<T>(
                        value =>
                        {
                            //--- ここはOnNext実行時に呼び出される
                            observer.OnNext(selector(value));
                        },
                        observer.OnError,       //--- OnErrorと
                        observer.OnCompleted)); //--- OnCompletedは何もせず素通し
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
    }



}
