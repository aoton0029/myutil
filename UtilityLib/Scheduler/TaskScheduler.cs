using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityLib.RetryHelper;

namespace UtilityLib.Scheduler
{
    internal class TaskScheduler
    {
        private readonly System.Threading.Timer _timer;
        private readonly Action _action;
        private readonly RetrySettings _retrySettings;
        private readonly TimeSpan _interval;
        private bool _isRunning;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTask"/> class.
        /// </summary>
        /// <param name="action">The action to be executed at regular intervals.</param>
        /// <param name="interval">The interval between each execution.</param>
        /// <param name="retrySettings">The settings to use for retrying the action in case of failure.</param>
        public TaskScheduler(Action action, TimeSpan interval, RetrySettings retrySettings = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _interval = interval;
            _retrySettings = retrySettings;
            _timer = new System.Threading.Timer(Execute, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Starts the scheduled task.
        /// </summary>
        public void Start()
        {
            _timer.Change(TimeSpan.Zero, _interval);
        }

        /// <summary>
        /// Stops the scheduled task.
        /// </summary>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void Execute(object state)
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    return;
                }
                _isRunning = true;
            }

            try
            {
                // Use the Retry class to execute the action with retry logic.
                Retry.WhileNot(() =>
                {
                    _action();
                    return true;
                }, result => result, _retrySettings);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that weren't handled by the retry logic.
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                lock (_lock)
                {
                    _isRunning = false;
                }
            }
        }
    }
