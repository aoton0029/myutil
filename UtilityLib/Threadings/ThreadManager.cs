using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Threadings
{
    public class ThreadManager
    {
        private List<ManagedThread> threads;
        private object lockObject = new object();

        public ThreadManager()
        {
            threads = new List<ManagedThread>();
        }

        public void StartNewThread(Action action, string threadName)
        {
            lock (lockObject)
            {
                ManagedThread managedThread = new ManagedThread(action, threadName);
                threads.Add(managedThread);
                managedThread.Start();
            }
        }

        public void StopThread(string threadName)
        {
            lock (lockObject)
            {
                ManagedThread thread = threads.Find(t => t.ThreadName == threadName);
                if (thread != null)
                {
                    thread.Stop();
                    threads.Remove(thread);
                }
            }
        }

        public void StopAllThreads()
        {
            lock (lockObject)
            {
                foreach (var thread in threads)
                {
                    thread.Stop();
                }
                threads.Clear();
            }
        }

        public void ShowThreadStatus()
        {
            lock (lockObject)
            {
                foreach (var thread in threads)
                {
                    Console.WriteLine($"Thread Name: {thread.ThreadName}, Is Running: {thread.IsRunning}");
                }
            }
        }

        private class ManagedThread
        {
            private Thread thread;
            private CancellationTokenSource cancellationTokenSource;
            public string ThreadName { get; private set; }
            public bool IsRunning { get; private set; }

            public ManagedThread(Action action, string threadName)
            {
                ThreadName = threadName;
                cancellationTokenSource = new CancellationTokenSource();
                thread = new Thread(() => Run(action, cancellationTokenSource.Token))
                {
                    IsBackground = true
                };
            }

            public void Start()
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    thread.Start();
                }
            }

            public void Stop()
            {
                if (IsRunning)
                {
                    cancellationTokenSource.Cancel();
                    IsRunning = false;
                }
            }

            private void Run(Action action, CancellationToken token)
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        action();
                        Thread.Sleep(100); // スレッドが他の処理に負荷をかけないように少し待機
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // スレッドが強制的に停止された場合の処理
                }
            }
        }
    }

    // 使用例
    public class Program
    {
        public static void Main()
        {
            ThreadManager threadManager = new ThreadManager();

            threadManager.StartNewThread(() =>
            {
                while (true)
                {
                    Console.WriteLine("Thread 1 is running...");
                    Thread.Sleep(1000);
                }
            }, "Thread1");

            threadManager.StartNewThread(() =>
            {
                while (true)
                {
                    Console.WriteLine("Thread 2 is running...");
                    Thread.Sleep(1500);
                }
            }, "Thread2");

            Thread.Sleep(5000); // 5秒待機
            threadManager.ShowThreadStatus();

            threadManager.StopAllThreads();
            threadManager.ShowThreadStatus();
        }
    }

}
