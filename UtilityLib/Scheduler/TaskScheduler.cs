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
        private readonly Func<bool> _action;
        private readonly RetrySettings _retrySettings;
        private readonly TimeSpan _interval;
        private bool _isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetriableTimer"/> class.
        /// </summary>
        /// <param name="action">The action to execute periodically. The action should return true if successful, false otherwise.</param>
        /// <param name="interval">The interval between each execution.</param>
        /// <param name="retrySettings">The settings to use for retrying the action if it fails.</param>
        public TaskScheduler(Func<bool> action, TimeSpan interval, RetrySettings retrySettings = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _interval = interval;
            _retrySettings = retrySettings ?? new RetrySettings
            {
                Timeout = TimeSpan.FromSeconds(5),
                Interval = TimeSpan.FromMilliseconds(500),
                ThrowOnTimeout = false,
                IgnoreException = true
            };
            _timer = new System.Threading.Timer(Execute, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            _timer.Change(TimeSpan.Zero, _interval);
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void Execute(object state)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            try
            {
                // Use the Retry class to retry the action if it fails
                var result = Retry.WhileFalse(_action, _retrySettings.Timeout, _retrySettings.Interval, _retrySettings.ThrowOnTimeout, _retrySettings.IgnoreException);
                if (!result.Success && result.HadException)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Action failed with exception: {result.LastException.Message}");
                }
            }
            finally
            {
                _isRunning = false;
            }
        }
    }
}