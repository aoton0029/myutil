了解です！
以下の2点を実装・統合します：


---

✅ 1. FailureCount を ScheduledTaskBase に追加

✅ 2. TaskSnapshot クラスを導入し、状態監視と外部UI/API連携に備える構造


---

1. 🔧 ScheduledTaskBase の拡張（FailureCount, ToSnapshot()）

public abstract class ScheduledTaskBase : TaskBase
{
    public TimeSpan Interval { get; }
    public int? MaxSkipCount { get; }
    public int RunCount { get; private set; }
    public int FailureCount { get; private set; }
    public DateTime? LastRun { get; private set; }
    public TimeSpan? LastDuration { get; private set; }

    public bool IsRunning { get; private set; }
    public bool IsFaulted { get; private set; }
    public Exception? LastException { get; private set; }

    private int _skipCount = 0;

    protected ScheduledTaskBase(string name, TimeSpan interval, int? maxSkipCount = null)
        : base(name)
    {
        Interval = interval;
        MaxSkipCount = maxSkipCount;
    }

    protected abstract Task ExecuteOnceAsync();

    protected override async Task ExecuteAsync()
    {
        var nextRunTime = DateTime.Now;
        IsRunning = true;
        IsFaulted = false;
        LastException = null;

        try
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                if (now < nextRunTime)
                {
                    await Task.Delay(nextRunTime - now, CancellationToken);
                }

                var start = DateTime.Now;
                await ExecuteOnceAsync();
                var end = DateTime.Now;

                RunCount++;
                LastRun = end;
                LastDuration = end - start;
                nextRunTime += Interval;

                if (LastDuration <= Interval)
                {
                    _skipCount = 0;
                    var wait = nextRunTime - DateTime.Now;
                    if (wait > TimeSpan.Zero)
                        await Task.Delay(wait, CancellationToken);
                }
                else
                {
                    _skipCount++;
                    ReportProgress($"スケジュール遅延によりスキップ {_skipCount} 回");

                    if (MaxSkipCount.HasValue && _skipCount > MaxSkipCount.Value)
                        throw new Exception($"スキップ回数が上限 {MaxSkipCount} を超えました");

                    nextRunTime = DateTime.Now + Interval;
                }
            }
        }
        catch (Exception ex)
        {
            FailureCount++;
            IsFaulted = true;
            LastException = ex;
            throw;
        }
        finally
        {
            IsRunning = false;
        }
    }

    public TaskSnapshot ToSnapshot()
    {
        return new TaskSnapshot
        {
            Name = Name,
            RunCount = RunCount,
            FailureCount = FailureCount,
            LastRun = LastRun,
            LastDuration = LastDuration,
            IsRunning = IsRunning,
            IsFaulted = IsFaulted,
            LastError = LastException?.Message
        };
    }
}


---

2. 📦 TaskSnapshot クラスの定義

public class TaskSnapshot
{
    public string Name { get; set; } = string.Empty;
    public int RunCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime? LastRun { get; set; }
    public TimeSpan? LastDuration { get; set; }
    public bool IsRunning { get; set; }
    public bool IsFaulted { get; set; }
    public string? LastError { get; set; }
}


---

3. 🧪 ScheduledTaskService から取得するメソッド例

public List<TaskSnapshot> GetTaskSnapshots()
{
    lock (_lock)
    {
        return _tasks
            .Select(kv => kv.Value.Task.ToSnapshot())
            .ToList();
    }
}


---

✨ 拡張提案（任意）


---

ご希望あれば TaskSnapshot を JSON 出力用に整形したり、UI 表示用のテンプレートを作ることも可能です。
何か追加しますか？




非常に重要なポイントです！
ここでは、TaskBase / ScheduledTaskBase を使ったときに例外が発生した場合のハンドリングを、TaskManagerService と TimerService のそれぞれでどう扱うべきかを整理し、具体的な実装例も提示します。


---

✅ 1. 共通設計ポリシー：例外発生時の分類


---

✅ 2. TaskBase / ScheduledTaskBase の内部構造（既定）

両者とも RunAsync 内部で try/catch を持ち、失敗時は TaskState.Failed にセットされ、
例外は上位に再スロー or 記録されるように設計されています。

try
{
    await ExecuteAsync();
    State = TaskState.Completed;
}
catch (Exception ex)
{
    LastError = ex;
    State = TaskState.Failed;
    throw;
}


---

✅ 3. TaskManagerService 側の例外ハンドリング

manager.TaskFailed += (s, result) =>
{
    var task = result.Task;
    var error = result.Exception;
    Console.WriteLine($"[Task Failed] {task.Name} - {error.Message}");

    // 例：ログ保存、再試行、UI通知など
    LogFailure(task, error);
    // Retry(task); ← オプションで再試行
};

実装アイデア：

即時再試行：リトライ回数付きで manager.Enqueue() 再投入

ログ記録：ファイル or DB に task.Name, error.Message 記録

エラーカウント：task.ErrorCount++ によって閾値超過検出



---

✅ 4. TimerService 側の例外ハンドリング（長期間動作）

timerService.TaskFailed += (s, e) =>
{
    Console.WriteLine($"[Scheduled Failed] {e.Name}: {e.Exception.Message}");

    if (autoRestartEnabled)
    {
        Console.WriteLine($"→ 自動再起動対象: {e.Name}");
        timerService.StartTask(GetTaskByName(e.Name));
    }
};

実装ポイント：

自動復旧：autoRestartEnabled == true で再登録（済）

スキップ上限対応：MaxSkipCount 超過時に通知＋再起動可否判断

失敗の可視化：モニターUIで「停止中」「失敗」の状態を表示



---

✅ 5. 失敗通知の共通イベント構造

どちらも TaskFailed イベントを持ち、内容は次のような型で定義されています：

public EventHandler<(TaskBase Task, Exception Exception)>? TaskFailed;

または ScheduledTask 用：

public EventHandler<(string Name, Exception Exception)>? TaskFailed;

推奨共通ハンドラ例：

void OnTaskFailed(object? sender, (string Name, Exception Exception) e)
{
    string name = e.Name;
    var ex = e.Exception;

    ShowErrorDialog(name, ex.Message);  // UI通知
    WriteLog(name, ex);                 // 永続ログ
}


---

✅ 6. 拡張案（おすすめ）


---

✅ まとめ


---

ご希望であれば、RetryPolicy を TaskBase に組み込んだバージョンや、エラー監視サービスの作成も可能です。
次に「再試行戦略」か「UI通知付きのエラーハンドラ」どちらか作ってみますか？





非常に重要な視点です！
TaskManagerService（TaskManager）と TimerService（TimerManager）をアプリ終了時や再起動時に安全に破棄（Dispose）する処理は、リソースリークやタスク暴走を防ぐために不可欠です。


---

✅ ゴール


---

✅ TaskManagerService の終了処理・破棄処理

実装に追加：

public class TaskManagerService : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _concurrentLimiter;
    private bool _disposed = false;

    public TaskManagerService(int maxConcurrency = 1)
    {
        _concurrentLimiter = new SemaphoreSlim(maxConcurrency);
    }

    public void CancelAll() => _cts.Cancel();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        _cts.Dispose();
        _concurrentLimiter.Dispose();
    }
}

呼び出し側（例：FormClosing）

protected override void OnFormClosing(FormClosingEventArgs e)
{
    taskManager.Dispose();
    base.OnFormClosing(e);
}


---

✅ TimerService の終了処理・破棄処理

1. 保持中の全 ScheduledTask を停止

2. CancellationTokenSource をすべて Cancel + Dispose

3. モニタータスク（ヘルスチェック）も終了

public class TimerService : IDisposable
{
    private readonly Dictionary<string, TaskHandle> _tasks = new();
    private readonly CancellationTokenSource _healthMonitorCts = new();
    private bool _disposed = false;

    public void StopAll()
    {
        lock (_tasks)
        {
            foreach (var handle in _tasks.Values)
            {
                handle.CancellationTokenSource.Cancel();
                handle.CancellationTokenSource.Dispose();
            }

            _tasks.Clear();
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


---

✅ 呼び出し例（フォーム/サービス終了時）

protected override void OnFormClosing(FormClosingEventArgs e)
{
    timerService.Dispose();
    base.OnFormClosing(e);
}


---

✅ 拡張：安全な Dispose 呼び出しのための型


---

✅ まとめ


---

✅ おまけ：WinForms アプリで一括終了処理する方法

Application.ApplicationExit += (s, e) =>
{
    taskManager.Dispose();
    timerService.Dispose();
};


---

必要なら、Dispose 呼び出し時にすべてのタスクの状態をログ保存してから終了するような「終了時永続保存」も組み込めます。
追加してみますか？





了解です！
TaskSnapshot 構造体は、TaskBase（または ScheduledTaskBase）の状態を一時的に記録・転送・保存するためのスナップショットデータ構造です。
ログ記録、UI表示、永続化（JSONなど）に非常に便利です。


---

✅ ゴール：TaskBase の状態をまとめた構造体


---

✅ TaskSnapshot 構造体の定義例

public readonly struct TaskSnapshot
{
    public string Name { get; init; }
    public Guid TaskId { get; init; }
    public TaskState State { get; init; }
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public TimeSpan? Duration => (StartTime.HasValue && EndTime.HasValue)
        ? EndTime - StartTime
        : null;

    public DateTime? LastProgressTime { get; init; }
    public string? LastErrorMessage { get; init; }
    public int? SkipCount { get; init; } // ScheduledTaskBase用など

    public string StatusText => State.ToString() + 
        (LastErrorMessage != null ? $" (Error: {LastErrorMessage})" : "");
}


---

✅ TaskBase → TaskSnapshot 変換メソッド（拡張メソッド）

public static class TaskSnapshotExtensions
{
    public static TaskSnapshot ToSnapshot(this TaskBase task)
    {
        return new TaskSnapshot
        {
            Name = task.Name,
            TaskId = task is IIdentifiable identifiable ? identifiable.Id : Guid.Empty,
            State = task.State,
            StartTime = task.StartTime,
            EndTime = task.EndTime,
            LastProgressTime = task.LastProgressTime,
            LastErrorMessage = task.LastError?.Message,
            SkipCount = task is ISkippable skippable ? skippable.SkipCount : null
        };
    }
}

※ IIdentifiable, ISkippable などは Guid Id や int SkipCount を持つ interface です。
→ 拡張性を持たせるなら導入おすすめ！


---

✅ 使用例（ログ出力）

var snapshot = myTask.ToSnapshot();
Console.WriteLine($"[{snapshot.State}] {snapshot.Name}: 実行時間 {snapshot.Duration?.TotalSeconds}s");


---

✅ 使用例（JSON保存）

var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("snapshot.json", json);


---

✅ 使用例（UI表示用）

BindingList<TaskSnapshot> _snapshots = new();
_snapshots.Add(task.ToSnapshot());

dataGridView.DataSource = _snapshots;


---

✅ 拡張案


---

必要なら、ScheduledTaskSnapshot を派生させて Interval や NextRunTime を持たせることもできます。
やってみましょうか？






良い質問です！
TaskSnapshot を「どこで持つか」は、その目的（表示、ログ、永続化、監視など）に応じて以下のように整理できます：


---

✅ 目的別：TaskSnapshot を持つ場所


---

✅ パターン1：UIやログ用に TaskManagerService 側で保持

public class TaskManagerService
{
    private readonly ConcurrentBag<TaskBase> _allTasks = new();
    private readonly List<TaskSnapshot> _snapshotHistory = new();

    public IEnumerable<TaskSnapshot> SnapshotHistory => _snapshotHistory.ToList();

    public event EventHandler<TaskSnapshot>? TaskSnapshotCreated;

    private void RecordSnapshot(TaskBase task)
    {
        var snap = task.ToSnapshot();
        _snapshotHistory.Add(snap);
        TaskSnapshotCreated?.Invoke(this, snap);
    }

    public void Enqueue(TaskBase task)
    {
        _allTasks.Add(task);

        _ = Task.Run(async () =>
        {
            try
            {
                await task.RunAsync(_cts.Token);
            }
            catch { }
            finally
            {
                RecordSnapshot(task);
            }
        });
    }
}


---

✅ パターン2：Form 側で BindingList<TaskSnapshot> を保持

private BindingList<TaskSnapshot> _snapshots = new();

private void InitializeSnapshotBinding()
{
    taskManager.TaskSnapshotCreated += (s, snap) =>
    {
        Invoke(() =>
        {
            _snapshots.Add(snap);
        });
    };

    dataGridView1.DataSource = _snapshots;
}


---

✅ パターン3：専用サービス TaskSnapshotStore を作る（分離設計）

public class TaskSnapshotStore
{
    private readonly List<TaskSnapshot> _snapshots = new();

    public void Add(TaskBase task) => _snapshots.Add(task.ToSnapshot());
    public IEnumerable<TaskSnapshot> All => _snapshots;
    public IEnumerable<TaskSnapshot> ByState(TaskState state) => _snapshots.Where(s => s.State == state);
}

→ TaskManagerService / TimerService は TaskSnapshotStore に依存注入して記録を委譲する構成も可能です。


---

✅ まとめ：どこで持つかの選び方


---

次に、どこで保持して何に使いたいか（UI表示・ログ・復元など）を教えてくれれば、それに合った構造をコードで出します！
たとえば「UIとログ両方に使いたい」なら ViewModel＋Store パターンも作れます。







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

