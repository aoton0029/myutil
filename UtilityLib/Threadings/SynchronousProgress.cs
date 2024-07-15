using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Threadings
{
    public class SynchronousProgress<T> : IProgress<T>
    {
        public event Action<T>? ProgressChanged;

        public SynchronousProgress(Action<T>? callback = null)
        {
            if (callback != null) ProgressChanged += callback;
        }

        void IProgress<T>.Report(T value) => OnReport(value);

        protected void OnReport(T value)
        {
            var callback = ProgressChanged;
            callback?.Invoke(value);
        }
    }
}
