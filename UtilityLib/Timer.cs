using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    // TimerTool
    public class TimerTool
    {
        private System.Timers.Timer timer;
        private int interval;
        private bool isRunning = false;
        public Action action { get; set; }
        public bool IsRunning { get { return isRunning; } }

        public TimerTool(int interval = 1000)
        {
            this.interval = interval;
            timer = new System.Timers.Timer(interval);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
        }

        public void Start()
        {
            if (!isRunning)
            {
                timer.Start();
                isRunning = true;
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
            }
        }
        
        public void ReStart()
        {
            Stop();
            Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            action?.Invoke();
        }
    }

    public class TimerInfo
    {
        /// <summary>
        /// The interval the wrapper Timer is currently set to
        /// </summary>
        public int Interval { get; protected set; }

        /// <summary>
        /// Whether the Timer is currently enabled and running - false if no Timer, or if interval is set to Timeout.Infinite
        /// </summary>
        public bool Active { get; protected set; }

        // Note: This property is thread-safe on 64-bit systems (since DateTimes are 64-bit and therefore atomic). On 32-bit systems, you could read an incomplete value.
        public DateTime LastFired { get; protected set; }

        protected System.Threading.Timer timer;

        protected Action timerCallback;



        /// <summary>
        /// Setup a TimerInfo object.
        /// </summary>
        /// <param name="timerCallback">The Action to call each time the Timer fires. Does away with a context object argument - use a closure instead.</param>
        public TimerInfo(Action timerCallback)
        {
            this.timerCallback = timerCallback;
            Interval = Timeout.Infinite;
            Active = false;
        }


        public void SetInterval(int milliseconds)
        {
            if (timer == null)
            {
                timer = new System.Threading.Timer(new TimerCallback(x =>
                {
                    LastFired = DateTime.UtcNow;
                    timerCallback();
                }));
            }

            Active = milliseconds != Timeout.Infinite;

            int dueTime = 0;
            if (!Active)
            {
                // If we're stopping the timer, dueTime should play along
                dueTime = Timeout.Infinite;
            }
            else
            {
                // If we're changing the timer interval from one int to another, dueTime should be the remainder of the new Interval
                if (Interval != Timeout.Infinite)
                {
                    dueTime = milliseconds - (int)(DateTime.UtcNow - LastFired).TotalMilliseconds;
                    if (dueTime < 0)
                        dueTime = 0;
                }
                //else
                //{
                // Wasn't even running - leave dueTime at 0
                //}
            }

            Interval = milliseconds;


            timer.Change(dueTime, milliseconds);
        }

        public void Stop()
        {
            SetInterval(Timeout.Infinite);
        }
    }


}
