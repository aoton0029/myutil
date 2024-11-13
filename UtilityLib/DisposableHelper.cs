using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    internal class DisposableHelper
    {
        public sealed class Disposable(Action callback) : IDisposable
        {
            /// <summary>
            /// Invokes the callback.
            /// </summary>
            public void Dispose() => callback();
        }

        public static void Dispose<T>(ref T obj) where T : class, IDisposable
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }
    }
}
