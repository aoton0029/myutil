了解です！
ここでは、長期間継続的に実行される ScheduledTaskBase の実例をご紹介します。
対象は数時間〜数日スパンで動作するような 監視・記録・収集系のタスク です。


---

✅ 長期間向きのタスク例（定期 + 軽処理）


---

✅ 実装例：SystemUsageLoggingTask

public class SystemUsageLoggingTask : ScheduledTaskBase
{
    private readonly string _logFilePath;

    public SystemUsageLoggingTask(string name, TimeSpan interval, string logFilePath, OverrunStrategy strategy = OverrunStrategy.Skip)
        : base(name, interval, strategy)
    {
        _logFilePath = logFilePath;
    }

    protected override Task ExecuteOnceAsync()
    {
        var cpuUsage = GetCpuUsage();     // 任意の実装
        var memUsage = GetMemoryUsage(); // 任意の実装

        string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CPU: {cpuUsage:F1}%, MEM: {memUsage:F1}%";
        File.AppendAllText(_logFilePath, log + Environment.NewLine);

        ReportProgress(log);
        return Task.CompletedTask;
    }

    // ダミー実装：本番では PerformanceCounter や WMI を使う
    private double GetCpuUsage() => new Random().NextDouble() * 100;
    private double GetMemoryUsage() => new Random().NextDouble() * 100;
}


---

✅ 実行例

var timerService = new TimerService(autoRestartFailed: true);

var sysLogTask = new SystemUsageLoggingTask(
    name: "SystemLogger",
    interval: TimeSpan.FromMinutes(10),
    logFilePath: @"C:\Logs\system_usage.log"
);

timerService.TaskStarted += (s, name) => Console.WriteLine($"[START] {name}");
timerService.TaskFailed += (s, e) => Console.WriteLine($"[ERROR] {e.Name}: {e.Exception.Message}");
timerService.StartTask(sysLogTask);


---

✅ 特徴


---

🧩 拡張案


---

必要なら cron 式で柔軟にスケジューリングする CronScheduledTaskBase なども作成可能です。
「夜間だけ動作」「15分に1回」などに柔軟対応できます。
このまま拡張してみましょうか？



了解です！
TimerService に UseLongRunningTask オプションを追加して、タスクごとに Task.Run or LongRunning を切り替えられるようにしてみましょう。


---

✅ 目的


---

✅ 拡張後の TimerService（抜粋）

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
    private readonly bool _autoRestartFailed;
    private readonly bool _useLongRunningTask;

    public event EventHandler<string>? TaskStarted;
    public event EventHandler<string>? TaskCompleted;
    public event EventHandler<(string Name, Exception Exception)>? TaskFailed;

    public TimerService(bool autoRestartFailed = true, bool useLongRunningTask = false)
    {
        _autoRestartFailed = autoRestartFailed;
        _useLongRunningTask = useLongRunningTask;
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

            if (_useLongRunningTask)
            {
                Task.Factory.StartNew(async () =>
                {
                    await RunScheduledTask(task, handle, cts);
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            else
            {
                _ = Task.Run(() => RunScheduledTask(task, handle, cts));
            }
        }
    }

    private async Task RunScheduledTask(ScheduledTaskBase task, TaskHandle handle, CancellationTokenSource cts)
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
    }

    // MonitorHealth(), StopTask(), etc. は従来通り
}


---

✅ 使用例（起動時にオプション指定）

// UseLongRunningTask を true にして TimerService を初期化
var timerService = new TimerService(autoRestartFailed: true, useLongRunningTask: true);

timerService.StartTask(new SystemUsageLoggingTask(
    name: "長時間稼働タスク",
    interval: TimeSpan.FromMinutes(10),
    logFilePath: @"C:\Logs\sys.log"
));


---

✅ 拡張ポイント


---

✅ まとめ


---

この設計で、TimerService を使うプロジェクト全体で「どのタスクをどう起動するか」を柔軟に選べるようになります。
必要があれば「タスクごとに切り替えられるバージョン」も作成できます。次はそれを作ってみましょうか？

