using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Threadings
{
    public class ThreadUtils 
    {
        public static Thread StartAsync(ThreadStart execute, [Localizable(false)] string? name = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            var thread = new Thread(execute) { Name = name };
            thread.Start();
            return thread;
        }

        public static Thread StartBackground(ThreadStart execute, [Localizable(false)] string? name = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            var thread = new Thread(execute) { Name = name, IsBackground = true };
            thread.Start();
            return thread;
        }

        public static void RunSta(Action execute)
        {
            #region Sanity checks
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            #endregion

            Exception? error = null;
            var thread = new Thread(new ThreadStart(delegate
            {
                try
                {
                    execute();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            //error?.Rethrow();
        }

        public static T RunSta<T>(Func<T> execute)
        {
            #region Sanity checks
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            #endregion

            T result = default!;
            Exception? error = null;
            var thread = new Thread(new ThreadStart(delegate
            {
                try
                {
                    result = execute();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            //error?.Rethrow();
            return result;
        }

    }
}
