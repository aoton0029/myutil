    public static class NativeEventWaiter
    {
        public static void WaitForEventLoop(string eventName, Action callback, Dispatcher dispatcher, CancellationToken cancel)
        {
            new Thread(() =>
            {
                var eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
                while (true)
                {
                    if (WaitHandle.WaitAny(new WaitHandle[] { cancel.WaitHandle, eventHandle }) == 1)
                    {
                        dispatcher.BeginInvoke(callback);
                    }
                    else
                    {
                        return;
                    }
                }
            }).Start();
        }
    }
