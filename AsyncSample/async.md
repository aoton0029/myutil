完璧な構成案ですね！
ここではテンプレートパターンとコマンドパターンを組み合わせ、「デバイスに対してシナリオを実行するタスク管理システム」 を構築します。


---

✅ 構成概要（目的）


---

✅ 1. ICommand（コマンドパターン）

public interface ICommand
{
    string Name { get; }
    Task<CommandResult> ExecuteAsync(IDevice device, CancellationToken cancellationToken);
}

public class CommandResult
{
    public bool Success { get; set; }
    public string Response { get; set; }
}


---

✅ 2. IDevice / IConnection

public interface IConnection
{
    Task SendAsync(string command, CancellationToken cancellationToken);
    Task<string> ReceiveAsync(CancellationToken cancellationToken);
}

public interface IDevice
{
    string Name { get; }
    IConnection Connection { get; }

    Task<CommandResult> SendCommandAsync(string command, CancellationToken cancellationToken);
}


---

✅ 3. ScenarioTemplate（テンプレートパターン）

public class ScenarioTemplate
{
    public string Name { get; set; }
    public List<ICommand> Commands { get; } = new();
    public Func<ScenarioResult, ScenarioTemplate?>? NextSelector { get; set; }  // 条件分岐可
}


---

✅ 4. ScenarioResult

public class ScenarioResult
{
    public string TemplateName { get; set; }
    public List<CommandResult> Results { get; set; } = new();
    public bool Success => Results.All(r => r.Success);
}


---

✅ 5. ScenarioTaskBase

public abstract class ScenarioTaskBase
{
    protected ScenarioTemplate Template { get; }
    protected IDevice Device { get; }

    public ScenarioTaskBase(ScenarioTemplate template, IDevice device)
    {
        Template = template;
        Device = device;
    }

    public async Task<ScenarioResult> RunAsync(CancellationToken cancellationToken)
    {
        var result = new ScenarioResult { TemplateName = Template.Name };

        foreach (var command in Template.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var r = await command.ExecuteAsync(Device, cancellationToken);
            result.Results.Add(r);

            if (!r.Success)
                break;
        }

        return result;
    }
}


---

✅ 6. ScenarioTaskManager

public class ScenarioTaskManager
{
    private readonly List<ScenarioTemplate> _templates = new();
    private readonly IDevice _device;

    public ScenarioTaskManager(IDevice device)
    {
        _device = device;
    }

    public void AddTemplate(ScenarioTemplate template)
    {
        _templates.Add(template);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        ScenarioTemplate? current = _templates.FirstOrDefault();

        while (current != null && !cancellationToken.IsCancellationRequested)
        {
            var task = new ConcreteScenarioTask(current, _device);
            var result = await task.RunAsync(cancellationToken);

            current = current.NextSelector?.Invoke(result);
        }
    }
}


---

✅ 7. ConcreteScenarioTask（必要なら）

public class ConcreteScenarioTask : ScenarioTaskBase
{
    public ConcreteScenarioTask(ScenarioTemplate template, IDevice device)
        : base(template, device) { }

    // 必要に応じてフックやログをここに追加可能
}


---

✅ 特徴と拡張性


---

✅ 次にできること

ICommand の具体例（SetVoltageCommand, ReadValueCommand など）

ScenarioTemplate を JSON や YAML で定義・読み込み

実行ログ（時刻、コマンド、レスポンス）保存機能

UI 上でシナリオの編集・再生・中断など操作



---

この構成で、高度な自動テスト・シナリオベース制御ができます！
必要ならこのまま UI とつないだサンプルや、テンプレート定義ファイル構文を作成しますか？





了解です！
ScenarioTaskManager / ScenarioTaskBase 実行時に、コマンド送信・応答・結果・時刻などを記録する 実行ログ（ExecutionLog）機能を導入しましょう。


---

✅ ゴール：実行ログに記録する内容


---

✅ 1. 実行ログモデル ExecutionLogEntry

public class ExecutionLogEntry
{
    public DateTime Timestamp { get; set; }
    public string TemplateName { get; set; } = "";
    public string CommandName { get; set; } = "";
    public string CommandText { get; set; } = "";
    public string Response { get; set; } = "";
    public bool Success { get; set; }
}


---

✅ 2. ログ記録サービス ExecutionLogger

public class ExecutionLogger
{
    private readonly List<ExecutionLogEntry> _entries = new();

    public void Log(ExecutionLogEntry entry)
    {
        _entries.Add(entry);
        Console.WriteLine($"[{entry.Timestamp:HH:mm:ss}] {entry.TemplateName} - {entry.CommandName}: {(entry.Success ? "OK" : "NG")}");
    }

    public IEnumerable<ExecutionLogEntry> GetEntries() => _entries;

    public void ExportJson(string path)
    {
        var json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public void Clear() => _entries.Clear();
}


---

✅ 3. ScenarioTaskBase にログ機能を追加

public abstract class ScenarioTaskBase
{
    protected ScenarioTemplate Template { get; }
    protected IDevice Device { get; }
    protected ExecutionLogger Logger { get; }

    public ScenarioTaskBase(ScenarioTemplate template, IDevice device, ExecutionLogger logger)
    {
        Template = template;
        Device = device;
        Logger = logger;
    }

    public async Task<ScenarioResult> RunAsync(CancellationToken cancellationToken)
    {
        var result = new ScenarioResult { TemplateName = Template.Name };

        foreach (var command in Template.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var commandResult = await command.ExecuteAsync(Device, cancellationToken);

            result.Results.Add(commandResult);

            Logger.Log(new ExecutionLogEntry
            {
                Timestamp = DateTime.Now,
                TemplateName = Template.Name,
                CommandName = command.Name,
                CommandText = (command as ITextCommand)?.Text ?? "N/A",
                Response = commandResult.Response,
                Success = commandResult.Success
            });

            if (!commandResult.Success)
                break;
        }

        return result;
    }
}

※ ITextCommand はオプション。コマンド文字列がわかるものに適用。


---

✅ 使用例

var logger = new ExecutionLogger();
var manager = new ScenarioTaskManager(device, logger);

await manager.RunAsync(CancellationToken.None);

// ログを保存
logger.ExportJson("run_log.json");


---

✅ 拡張案


---

実行ログを UI や CSV にも出したい、という場合はすぐ対応できます。
次は UI連携 or CSVログ出力 を作成しましょうか？



了解です！
ScenarioTaskManager のシナリオ進行状況（プログレス）を WinForms の UI にリアルタイム表示する構成を作りましょう。


---

✅ ゴール：UIでのプログレス表示


---

✅ 構成概要

[ScenarioTaskManager]
 └→ プログレスイベント発行（OnProgress）
[Form]
 └→ UI更新（Invokeでコントロール更新）


---

✅ 1. プログレスイベント用モデル

public class ScenarioProgress
{
    public string TemplateName { get; set; } = "";
    public string CommandName { get; set; } = "";
    public int CommandIndex { get; set; }
    public int TotalCommands { get; set; }
    public bool? Success { get; set; }
    public string Message { get; set; } = "";
}


---

✅ 2. ScenarioTaskBase にイベント追加

public event EventHandler<ScenarioProgress>? ProgressChanged;

protected void ReportProgress(ScenarioProgress progress)
{
    ProgressChanged?.Invoke(this, progress);
}


---

✅ 3. プログレス発行の追加（コマンド実行中）

for (int i = 0; i < Template.Commands.Count; i++)
{
    var command = Template.Commands[i];
    cancellationToken.ThrowIfCancellationRequested();

    var result = await command.ExecuteAsync(Device, cancellationToken);

    resultList.Add(result);

    ReportProgress(new ScenarioProgress
    {
        TemplateName = Template.Name,
        CommandName = command.Name,
        CommandIndex = i + 1,
        TotalCommands = Template.Commands.Count,
        Success = result.Success,
        Message = result.Response
    });

    if (!result.Success) break;
}


---

✅ 4. UI側でバインディング

フォームメンバ：

private ScenarioTaskManager _manager;
private Label lblTemplate;
private Label lblCommand;
private ProgressBar progressBar;
private TextBox txtLog;

初期化時のハンドラ登録：

task.ProgressChanged += (s, p) =>
{
    Invoke(() =>
    {
        lblTemplate.Text = $"テンプレート: {p.TemplateName}";
        lblCommand.Text = $"コマンド: {p.CommandName}";
        progressBar.Maximum = p.TotalCommands;
        progressBar.Value = p.CommandIndex;

        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {p.CommandName}: {p.Message}\r\n");
    });
};


---

✅ 拡張：DataGridViewでログ表示する場合

BindingList<ScenarioProgress> _progressLog = new();
dataGridView1.DataSource = _progressLog;

task.ProgressChanged += (s, p) =>
{
    Invoke(() => _progressLog.Add(p));
};


---

✅ まとめ


---

✅ 次の拡張案


---

この構成で、リアルタイムでシナリオ進行を見える化するUIが作れます。
ご希望があれば、フォームテンプレート or Designer付きの完全サンプルも作成します！続けますか？




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

