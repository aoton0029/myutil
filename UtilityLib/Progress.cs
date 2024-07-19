using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class ProgressUtility
    {
        public static IDisposable StartProgress(Action<ProgressEventArgs> progressHandler)
        {
            var sw = Stopwatch.StartNew();
            return Delegate.Disposable(() =>
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
