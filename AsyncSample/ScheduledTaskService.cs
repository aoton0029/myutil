using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSample
{
    public class ScheduledTaskService : IDisposable
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
        private readonly IProgress<TaskSnapshot>? _snapshotProgress;
        private bool _disposed = false;

        public event EventHandler<string>? TaskStarted;
        public event EventHandler<string>? TaskCompleted;
        public event EventHandler<(string Name, Exception Exception)>? TaskFailed;

        public ScheduledTaskService(bool autoRestartFailed = true)
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
                        await task.RunAsync(cts.Token, _snapshotProgress);
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
                {
                    handle.CancellationTokenSource.Cancel();
                    handle.CancellationTokenSource.Dispose();
                }

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

        public List<TaskSnapshot> GetMonitorInfoList()
        {
            lock (_lock)
            {
                return _tasks.Select(kv =>
                {
                    var t = kv.Value;
                    return new TaskSnapshot();
                }).ToList();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopAll();
            _healthMonitorCts.Cancel();
            _healthMonitorCts.Dispose();
        }
    }

}
