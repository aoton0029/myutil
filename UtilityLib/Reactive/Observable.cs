using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    static class Observable
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source,
                                               Action<T> onNext,
                                               Action<T> onBack,
                                               Action<Exception>? onError = null,
                                               Action? onCompleted = null,
                                               Action? onCanceled = null,
                                               Action? onTerminated = null) 
            => source.Subscribe(Delegates.Observer(onNext, onBack, onError, onCompleted, onCanceled, onTerminated));
    }


}
