using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AsyncSample.Form2;

namespace AsyncSample
{
    public class ScheduledTaskService : IDisposable
    {
        private class TaskHandle
        {
            public ScheduledTaskBase Task { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public bool IsRunning { get; set; }
            public Task? RunningTask { get; set; }
            public Exception? LastError { get; set; }
        }

        private readonly ConcurrentDictionary<string, TaskHandle> _tasks = new();
        private readonly object _lock = new();
        private readonly bool _autoRestartFailed;
        private readonly IProgress<TaskSnapshot>? _snapshotProgress;
        private readonly ScheduledTaskMonitor _monitor;
        private bool _disposed = false;

        public event EventHandler<string>? TaskStarted;
        public event EventHandler<string>? TaskCompleted;
        public event EventHandler<(string Name, Exception Exception)>? TaskFailed;

        public ScheduledTaskService(bool autoRestartFailed = true, IProgress<TaskSnapshot>? snapshotProgress = null)
        {
            _snapshotProgress = snapshotProgress;
            _autoRestartFailed = autoRestartFailed;
            _monitor = new ScheduledTaskMonitor(this, TimeSpan.FromSeconds(10));
            _monitor.Start();
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
                    IsRunning = true
                };

                var runningTask = Task.Factory.StartNew(async () =>
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
                        handle.IsRunning = false;
                    }
                },
                cts.Token,
                TaskCreationOptions.LongRunning, // 長時間タスクを示す
                TaskScheduler.Default            // 明示的にスレッドプール利用
                ).Unwrap();

                handle.RunningTask = runningTask;
                _tasks[task.Name] = handle;
            }
        }

        public void StopTask(string name)
        {
                if (_tasks.TryGetValue(name, out var handle))
                {
                    handle.CancellationTokenSource.Cancel();
                    handle.IsRunning = false;
                }
        }

        public void StopAll()
        {
                foreach (var handle in _tasks.Values)
                {
                    handle.CancellationTokenSource.Cancel();
                    //handle.CancellationTokenSource.Dispose();
                }

                _tasks.Clear();
        }

        public List<string> ListRunningTasks()
        {
                return _tasks
                    .Where(kv => kv.Value.IsRunning)
                    .Select(kv => kv.Key)
                    .ToList();
        }

        public void CheckTaskHealth()
        {
                foreach (var (name, handle) in _tasks)
                {
                    if (!handle.IsRunning && handle.RunningTask?.IsCompleted == true)
                    {
                        Debug.Print($"[HEALTH] タスク '{name}' は停止中");

                        if (_autoRestartFailed)
                        {
                            Debug.Print($"[HEALTH] タスク '{name}' を再起動します");
                            StartTask(handle.Task);
                        }
                    }
                }
        }

        public List<TaskMonitorInfo> GetMonitorInfoList()
        {
                return _tasks.Select(kv =>
                {
                    var t = kv.Value;
                    return new TaskMonitorInfo();
                }).ToList();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopAll();
            _monitor.Dispose();
        }

        public bool TryStart(ScheduledTaskBase task)
        {
            if (_tasks.ContainsKey(task.Name)) return false;

            var cts = new CancellationTokenSource();
            var handle = new TaskHandle
            {
                Task = task,
                CancellationTokenSource = cts,
                IsRunning = true
            };

            var runningTask = Task.Run(async () =>
            {
                try
                {
                    await task.RunAsync(cts.Token);
                }
                catch (Exception ex)
                {
                    handle.LastError = ex;
                }
                finally
                {
                    handle.IsRunning = false;
                }
            }, cts.Token);

            handle.RunningTask = runningTask;
            return _tasks.TryAdd(task.Name, handle);
        }

        public async Task<bool> TryRestartAsync(string name)
        {
            if (!_tasks.TryGetValue(name, out var existingHandle))
                return false;

            var task = existingHandle.Task;

            var stopped = await TryStopAsync(name);
            if (!stopped) return false;

            return TryStart(task);
        }


        public async Task<bool> TryStopAsync(string name)
        {
            if (!_tasks.TryRemove(name, out var handle)) return false;

            handle.CancellationTokenSource.Cancel();
            handle.CancellationTokenSource.Dispose();

            if (handle.RunningTask != null)
            {
                try { await handle.RunningTask; }
                catch { /* swallow */ }
            }

            return true;
        }

        public async Task StopAllAsync()
        {
            var tasks = _tasks.Keys.ToList();
            foreach (var name in tasks)
            {
                await TryStopAsync(name);
            }
        }

        public List<TaskSnapshot> GetSnapshots()
        {
            return _tasks.Values.Select(h => new TaskSnapshot
            {
                Name = h.Task.Name,
                Status = h.IsRunning
                    ? "Running"
                    : h.LastError != null ? "Faulted" : "Stopped",
                LastRunTime = h.Task.LastRun ?? DateTime.MinValue,
                RunCount = h.Task.RunCount,
                ErrorMessage = h.LastError?.Message
            }).ToList();
        }

        public bool IsRunning(string name)
        {
            return _tasks.TryGetValue(name, out var handle) && handle.IsRunning;
        }

        public bool IsStopped(string name)
        {
            return !_tasks.ContainsKey(name) || !_tasks[name].IsRunning;
        }
    }

    public class ScheduledTaskMonitor
    {
        private readonly ScheduledTaskService _service;
        private readonly TimeSpan _interval;
        private readonly CancellationTokenSource _cts = new();
        private Task? _monitoringTask;

        public ScheduledTaskMonitor(ScheduledTaskService service, TimeSpan interval)
        {
            _service = service;
            _interval = interval;
        }

        public void Start()
        {
            _monitoringTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    _service.CheckTaskHealth();
                    await Task.Delay(_interval, _cts.Token);
                }
            });
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }
}
