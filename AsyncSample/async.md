素晴らしい選択です！
ここでは、以下の2種類のタスクを TaskBase を使って実装します：


---

✅ 実装する2種類のタスク


---

1. 🔧 LargeFileCopyTask

public class LargeFileCopyTask : TaskBase
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private const int BufferSize = 81920;

    public LargeFileCopyTask(string name, string source, string destination)
        : base(name)
    {
        _sourcePath = source;
        _destinationPath = destination;
    }

    protected override async Task ExecuteAsync()
    {
        if (!File.Exists(_sourcePath))
            throw new FileNotFoundException("Source file not found", _sourcePath);

        var fileInfo = new FileInfo(_sourcePath);
        long totalSize = fileInfo.Length;
        long copied = 0;

        ReportProgress($"[{Name}] コピー開始: {fileInfo.Name} ({totalSize / 1024 / 1024} MB)");

        using var source = new FileStream(_sourcePath, FileMode.Open, FileAccess.Read);
        using var dest = new FileStream(_destinationPath, FileMode.Create, FileAccess.Write);

        byte[] buffer = new byte[BufferSize];
        int read;

        while ((read = await source.ReadAsync(buffer, 0, buffer.Length, CancellationToken)) > 0)
        {
            await dest.WriteAsync(buffer, 0, read, CancellationToken);
            copied += read;

            double percent = (double)copied / totalSize * 100;
            ReportProgress($"[{Name}] 進捗: {percent:F2}% ({copied}/{totalSize} bytes)");
            await Task.Yield(); // 他の処理をブロックしない
        }

        ReportProgress($"[{Name}] コピー完了: {Path.GetFileName(_destinationPath)}");
    }
}


---

2. 🔧 LargeAmountFileCopyTask

public class LargeAmountFileCopyTask : TaskBase
{
    private readonly string _sourceDir;
    private readonly string _destinationDir;

    public LargeAmountFileCopyTask(string name, string sourceDir, string destinationDir)
        : base(name)
    {
        _sourceDir = sourceDir;
        _destinationDir = destinationDir;
    }

    protected override async Task ExecuteAsync()
    {
        if (!Directory.Exists(_sourceDir))
            throw new DirectoryNotFoundException("Source directory not found: " + _sourceDir);

        Directory.CreateDirectory(_destinationDir);

        var files = Directory.GetFiles(_sourceDir, "*.*", SearchOption.TopDirectoryOnly);
        int total = files.Length;
        int copied = 0;

        ReportProgress($"[{Name}] ファイルコピー開始（{total}件）");

        foreach (var sourceFile in files)
        {
            CancellationToken.ThrowIfCancellationRequested();

            string destFile = Path.Combine(_destinationDir, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, destFile, true);

            copied++;
            double percent = (double)copied / total * 100;
            ReportProgress($"[{Name}] コピー中: {percent:F1}% ({copied}/{total})");
            await Task.Yield();
        }

        ReportProgress($"[{Name}] 全ファイルコピー完了（{copied}件）");
    }
}


---

✅ 使用例（TaskManagerService に登録）

var manager = new TaskManagerService();

manager.TaskStarted += (s, t) => Console.WriteLine($"開始: {t.Name}");
manager.TaskCompleted += (s, t) => Console.WriteLine($"完了: {t.Name}");

var largeFileTask = new LargeFileCopyTask("大容量ファイルコピー", @"C:\BigData.iso", @"D:\Backup\BigData.iso");
manager.Enqueue(largeFileTask);

var multiCopyTask = new LargeAmountFileCopyTask("フォルダ全コピー", @"C:\Images", @"D:\Backup\Images");
manager.Enqueue(multiCopyTask);


---

✅ 特徴


---

🧩 拡張案


---

必要であれば、WinForms UI と連携した進捗バー付きコピー画面 や、コピー条件設定ダイアログなども作れます。
組み込み先や目的に合わせて、さらに具体化しましょうか？





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

