using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    /// <summary>
    /// 情報の受信者としての機能を提供します。
    /// </summary>
    /// <typeparam name="T">受信するデータの型</typeparam>
    class EndPointObserver<T> : IObserver<T>
    {
        private readonly Action onCompleted = null;
        private readonly Action<Exception> onError = null;
        private readonly Action<T> onNext = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="onNext">OnNextのときに呼び出されるデリゲート</param>
        /// <param name="onError">OnErrorのときに呼び出されるデリゲート</param>
        /// <param name="onCompleted">OnCompletedのときに呼び出されるデリゲート</param>
        public EndPointObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            this.onNext = onNext ?? throw new ArgumentNullException("onNext");
            this.onError = onError ?? throw new ArgumentNullException("onError");
            this.onCompleted = onCompleted ?? throw new ArgumentNullException("onCompleted");
        }

        /// <summary>
        /// プロバイダーがプッシュベースの通知の送信を完了したことをオブザーバーに通知します。
        /// </summary>
        public void OnCompleted()
        {
            this.onCompleted();
        }

        /// <summary>
        /// プロバイダーでエラー状態が発生したことをオブザーバーに通知します。
        /// </summary>
        /// <param name="error">エラーに関する追加情報を提供するオブジェクト</param>
        public void OnError(Exception error)
        {
            this.onError(error);
        }

        /// <summary>
        /// オブザーバーに新しいデータを提供します。
        /// </summary>
        /// <param name="value">現在の通知情報</param>
        public void OnNext(T value)
        {
            this.onNext(value);
        }
    }


    /// <summary>
    /// 情報の受信者としての機能を提供します。
    /// </summary>
    /// <typeparam name="T">受信するデータの型</typeparam>
    class AnonymousObserver<T> : IObserver<T>
    {
        private readonly Action onCompleted = null;
        private readonly Action<Exception> onError = null;
        private readonly Action<T> onNext = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="onNext">OnNextのときに呼び出されるデリゲート</param>
        /// <param name="onError">OnErrorのときに呼び出されるデリゲート</param>
        /// <param name="onCompleted">OnCompletedのときに呼び出されるデリゲート</param>
        public AnonymousObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            this.onNext = onNext ?? throw new ArgumentNullException("onNext");
            this.onError = onError ?? throw new ArgumentNullException("onError");
            this.onCompleted = onCompleted ?? throw new ArgumentNullException("onCompleted");
        }

        /// <summary>
        /// プロバイダーがプッシュベースの通知の送信を完了したことをオブザーバーに通知します。
        /// </summary>
        public void OnCompleted()
        {
            this.onCompleted();
        }

        /// <summary>
        /// プロバイダーでエラー状態が発生したことをオブザーバーに通知します。
        /// </summary>
        /// <param name="error">エラーに関する追加情報を提供するオブジェクト</param>
        public void OnError(Exception error)
        {
            this.onError(error);
        }

        /// <summary>
        /// オブザーバーに新しいデータを提供します。
        /// </summary>
        /// <param name="value">現在の通知情報</param>
        public void OnNext(T value)
        {
            this.onNext(value);
        }
    }

    /// <summary>
    /// 情報の受信者としての機能を提供します。
    /// </summary>
    /// <typeparam name="T">受信するデータの型</typeparam>
    class SelectObserver<T> : IObserver<T>
    {
        private readonly Action onCompleted = null;
        private readonly Action<Exception> onError = null;
        private readonly Action<T> onNext = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="onNext">OnNextのときに呼び出されるデリゲート</param>
        /// <param name="onError">OnErrorのときに呼び出されるデリゲート</param>
        /// <param name="onCompleted">OnCompletedのときに呼び出されるデリゲート</param>
        public SelectObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            this.onNext = onNext ?? throw new ArgumentNullException("onNext");
            this.onError = onError ?? throw new ArgumentNullException("onError");
            this.onCompleted = onCompleted ?? throw new ArgumentNullException("onCompleted");
        }

        /// <summary>
        /// プロバイダーがプッシュベースの通知の送信を完了したことをオブザーバーに通知します。
        /// </summary>
        public void OnCompleted()
        {
            this.onCompleted();
        }

        /// <summary>
        /// プロバイダーでエラー状態が発生したことをオブザーバーに通知します。
        /// </summary>
        /// <param name="error">エラーに関する追加情報を提供するオブジェクト</param>
        public void OnError(Exception error)
        {
            this.onError(error);
        }

        /// <summary>
        /// オブザーバーに新しいデータを提供します。
        /// </summary>
        /// <param name="value">現在の通知情報</param>
        public void OnNext(T value)
        {
            this.onNext(value);
        }
    }

    /// <summary>
    /// 情報の受信者としての機能を提供します。
    /// </summary>
    /// <typeparam name="T">受信するデータの型</typeparam>
    class WhereObserver<T> : IObserver<T>
    {
        private readonly Action onCompleted = null;
        private readonly Action<Exception> onError = null;
        private readonly Action<T> onNext = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="onNext">OnNextのときに呼び出されるデリゲート</param>
        /// <param name="onError">OnErrorのときに呼び出されるデリゲート</param>
        /// <param name="onCompleted">OnCompletedのときに呼び出されるデリゲート</param>
        public WhereObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            this.onNext = onNext ?? throw new ArgumentNullException("onNext");
            this.onError = onError ?? throw new ArgumentNullException("onError");
            this.onCompleted = onCompleted ?? throw new ArgumentNullException("onCompleted");
        }

        /// <summary>
        /// プロバイダーがプッシュベースの通知の送信を完了したことをオブザーバーに通知します。
        /// </summary>
        public void OnCompleted()
        {
            this.onCompleted();
        }

        /// <summary>
        /// プロバイダーでエラー状態が発生したことをオブザーバーに通知します。
        /// </summary>
        /// <param name="error">エラーに関する追加情報を提供するオブジェクト</param>
        public void OnError(Exception error)
        {
            this.onError(error);
        }

        /// <summary>
        /// オブザーバーに新しいデータを提供します。
        /// </summary>
        /// <param name="value">現在の通知情報</param>
        public void OnNext(T value)
        {
            this.onNext(value);
        }
    }

    /// <summary>
    /// 情報通知のプロバイダーとしての機能を提供します。
    /// </summary>
    /// <typeparam name="T">受信するデータの型</typeparam>
    class AnonymousObservable<T> : IObservable<T>
    {
        private readonly Func<IObserver<T>, IDisposable> subscribe = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="subscribe">Subscribeのときに呼び出されるデリゲート</param>
        public AnonymousObservable(Func<IObserver<T>, IDisposable> subscribe)
        {
            this.subscribe = subscribe ?? throw new ArgumentNullException("subscribe");
        }

        /// <summary>
        /// オブザーバーが通知を受け取ることをプロバイダーに通知します。
        /// </summary>
        /// <param name="observer">通知を受け取るオブジェクト</param>
        /// <returns>プロバイダーが通知の送信を完了する前に、オブザーバーが通知の受信を停止できるインターフェイスへの参照</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.subscribe(observer);
        }
    }

    /// <summary>
    /// 情報通知のプロバイダーとしての機能を提供します。
    /// </summary>
    /// <typeparam name="T">受信するデータの型</typeparam>
    class WhereObservable<T> : IObservable<T>
    {
        private readonly Func<IObserver<T>, IDisposable> subscribe = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="subscribe">Subscribeのときに呼び出されるデリゲート</param>
        public WhereObservable(Func<IObserver<T>, IDisposable> subscribe)
        {
            this.subscribe = subscribe ?? throw new ArgumentNullException("subscribe");
        }

        /// <summary>
        /// オブザーバーが通知を受け取ることをプロバイダーに通知します。
        /// </summary>
        /// <param name="observer">通知を受け取るオブジェクト</param>
        /// <returns>プロバイダーが通知の送信を完了する前に、オブザーバーが通知の受信を停止できるインターフェイスへの参照</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.subscribe(observer);
        }
    }

    /// <summary>
    /// 情報通知のプロバイダーとしての機能を提供します。
    /// </summary>
    /// <typeparam name="T">受信するデータの型</typeparam>
    class SelectObservable<T> : IObservable<T>
    {
        private readonly Func<IObserver<T>, IDisposable> subscribe = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="subscribe">Subscribeのときに呼び出されるデリゲート</param>
        public SelectObservable(Func<IObserver<T>, IDisposable> subscribe)
        {
            this.subscribe = subscribe ?? throw new ArgumentNullException("subscribe");
        }

        /// <summary>
        /// オブザーバーが通知を受け取ることをプロバイダーに通知します。
        /// </summary>
        /// <param name="observer">通知を受け取るオブジェクト</param>
        /// <returns>プロバイダーが通知の送信を完了する前に、オブザーバーが通知の受信を停止できるインターフェイスへの参照</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.subscribe(observer);
        }
    }

    public static class Extension
    {
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
    }
}
