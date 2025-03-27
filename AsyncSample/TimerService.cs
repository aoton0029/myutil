using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSample
{
    public enum OverrunStrategy
    {
        FixedInterval,    // 固定間隔（前回終了から一定間隔）
        CatchUp,          // スケジュール基準で即時実行
        Skip              // スケジュール基準、次の時刻までスキップ
    }

    public class TaskMonitorInfo
    {
        public string Name { get; set; }
        public bool IsRunning { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string LastStatus { get; set; }
    }

    public abstract class ScheduledTaskBase : TaskBase
    {
        public TimeSpan Interval { get; }
        public OverrunStrategy OverrunStrategy { get; }
        public int? MaxSkipCount { get; }
        public int RunCount { get; set; }
        public DateTime? LastRun { get; set; }

        private int _skipCount = 0;

        protected ScheduledTaskBase(
            string name,
            TimeSpan interval,
            OverrunStrategy overrunStrategy = OverrunStrategy.FixedInterval,
            int? maxSkipCount = null)
            : base(name)
        {
            Interval = interval;
            OverrunStrategy = overrunStrategy;
            MaxSkipCount = maxSkipCount;
        }

        protected abstract Task ExecuteOnceAsync();

        protected override async Task ExecuteAsync()
        {
            var nextRunTime = DateTime.Now;

            while (!CancellationToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (OverrunStrategy == OverrunStrategy.Skip && now < nextRunTime)
                {
                    await Task.Delay(nextRunTime - now, CancellationToken);
                }

                var actualStart = DateTime.Now;
                await ExecuteOnceAsync();
                var actualEnd = DateTime.Now;

                switch (OverrunStrategy)
                {
                    case OverrunStrategy.FixedInterval:
                        var waitTime = Interval - (actualEnd - actualStart);
                        if (waitTime > TimeSpan.Zero)
                            await Task.Delay(waitTime, CancellationToken);
                        break;

                    case OverrunStrategy.CatchUp:
                        nextRunTime += Interval;
                        break;

                    case OverrunStrategy.Skip:
                        nextRunTime += Interval;

                        if (nextRunTime < DateTime.Now)
                        {
                            _skipCount++;
                            ReportProgress($"スケジュール遅延によりスキップ {_skipCount} 回");

                            if (MaxSkipCount.HasValue && _skipCount > MaxSkipCount.Value)
                            {
                                ReportProgress($"スキップ回数上限超過: {_skipCount} 回");
                                throw new Exception($"スキップ回数が上限 {MaxSkipCount} を超えました");
                            }

                            nextRunTime = DateTime.Now + Interval;
                        }
                        else
                        {
                            _skipCount = 0; // 実行できたのでリセット
                        }
                        break;
                }
            }
        }
    }


    public class TimerService
    {
        private class TaskHandle
        {
            public ScheduledTaskBase Task { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public DateTime LastRunTime { get; set; }
            public bool IsRunning { get; set; }
        }

        private readonly Dictionary<string, TaskHandle> _tasks = new();
        private readonly object _lock = new();
        private readonly TimeSpan _healthCheckInterval = TimeSpan.FromSeconds(10);
        private readonly bool _autoRestartFailed;

        public event EventHandler<string>? TaskStarted;
        public event EventHandler<string>? TaskCompleted;
        public event EventHandler<(string Name, Exception Exception)>? TaskFailed;

        public TimerService(bool autoRestartFailed = true)
        {
            _autoRestartFailed = autoRestartFailed;
            StartHealthMonitor();
        }

        public void StartTask(ScheduledTaskBase task)
        {
            lock (_lock)
            {
                if (_tasks.ContainsKey(task.Name))
                    return;

                var cts = new CancellationTokenSource();
                var handle = new TaskHandle
                {
                    Task = task,
                    CancellationTokenSource = cts,
                    LastRunTime = DateTime.MinValue,
                    IsRunning = true
                };

                _tasks[task.Name] = handle;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        TaskStarted?.Invoke(this, task.Name);
                        await task.RunAsync(cts.Token);
                        TaskCompleted?.Invoke(this, task.Name);
                    }
                    catch (Exception ex)
                    {
                        TaskFailed?.Invoke(this, (task.Name, ex));
                    }
                    finally
                    {
                        handle.LastRunTime = DateTime.Now;
                        handle.IsRunning = false;
                    }
                });
            }
        }

        public void StopTask(string name)
        {
            lock (_lock)
            {
                if (_tasks.TryGetValue(name, out var handle))
                {
                    handle.CancellationTokenSource.Cancel();
                    handle.IsRunning = false;
                    _tasks.Remove(name);
                }
            }
        }

        public void StopAll()
        {
            lock (_lock)
            {
                foreach (var handle in _tasks.Values)
                    handle.CancellationTokenSource.Cancel();

                _tasks.Clear();
            }
        }

        public List<string> ListRunningTasks()
        {
            lock (_lock)
            {
                return _tasks
                    .Where(kv => kv.Value.IsRunning)
                    .Select(kv => kv.Key)
                    .ToList();
            }
        }

        public void MonitorHealth()
        {
            lock (_lock)
            {
                foreach (var (name, handle) in _tasks)
                {
                    if (!handle.IsRunning)
                    {
                        Console.WriteLine($"[HEALTH] タスク '{name}' は停止中");

                        if (_autoRestartFailed)
                        {
                            Console.WriteLine($"[HEALTH] タスク '{name}' を再起動します");
                            StartTask(handle.Task);
                        }
                    }
                }
            }
        }

        private void StartHealthMonitor()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(_healthCheckInterval);
                    MonitorHealth();
                }
            });
        }

        public List<TaskMonitorInfo> GetMonitorInfoList()
        {
            lock (_lock)
            {
                return _tasks.Select(kv =>
                {
                    var t = kv.Value;
                    return new TaskMonitorInfo
                    {
                        Name = kv.Key,
                        IsRunning = t.IsRunning,
                        LastRunTime = t.LastRunTime,
                        LastStatus = t.IsRunning ? "Running" :
                                      t.LastRunTime == DateTime.MinValue ? "Never Started" : "Stopped"
                    };
                }).ToList();
            }
        }

    }

}
