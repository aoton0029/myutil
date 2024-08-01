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

        public static void RunTask(Func<Task> action)
        {
            var synchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                action().Wait();
            }
            catch (AggregateException ex)
            {
                throw;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }

        }

        public static T RunTask<T>(Func<Task<T>> action)
        {
            var synchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                return action().Result;
            }
            catch (AggregateException ex)
            {
                throw;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }
        }
    }
}
