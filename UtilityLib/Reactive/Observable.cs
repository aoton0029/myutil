using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    
    sealed class NopDisposable : IDisposable
    {
        public static readonly NopDisposable Instance = new NopDisposable();
        NopDisposable() { }
        public void Dispose() { }
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

    public class ProgressUtility
    {
        public static IDisposable StartProgress(Action<ProgressEventArgs> progressHandler)
        {
            var sw = Stopwatch.StartNew();
            return ObservableExtensions.Disposable(() =>
            {
                sw.Stop();
                progressHandler(new ProgressEventArgs(sw.ElapsedMilliseconds, sw.ElapsedMilliseconds));
            });
        }

        public delegate void ProgressHandler(object sender, ProgressEventArgs e);

        public class ProgressEventArgs
        {
            public ProgressEventArgs()
            {

            }

            public ProgressEventArgs(long current, long total)
            {
                Current = current;
                Total = total;
            }
            public long Current { get; set; }
            public long Total { get; set; }
        }

    }

}
