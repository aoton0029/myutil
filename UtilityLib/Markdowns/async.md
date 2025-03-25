了解です！
ここでは、定期実行用の TaskBase 派生クラス と、それを制御する TimerService（定期実行マネージャ） を構築します。


---

✅ 目的


---

1. 🔧 ScheduledTaskBase の定義

public abstract class ScheduledTaskBase : TaskBase
{
    public TimeSpan Interval { get; }

    protected ScheduledTaskBase(string name, TimeSpan interval)
        : base(name)
    {
        Interval = interval;
    }

    /// <summary>
    /// 1回分の処理
    /// </summary>
    protected abstract Task ExecuteOnceAsync();

    protected override async Task ExecuteAsync()
    {
        while (!CancellationToken.IsCancellationRequested)
        {
            var start = DateTime.Now;
            await ExecuteOnceAsync();
            var elapsed = DateTime.Now - start;

            var waitTime = Interval - elapsed;
            if (waitTime > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(waitTime, CancellationToken);
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}


---

2. 🔧 TimerService（定期タスクを管理）

public class TimerService
{
    private readonly Dictionary<string, CancellationTokenSource> _cancellationMap = new();
    private readonly object _lock = new();

    public void StartTask(ScheduledTaskBase task)
    {
        lock (_lock)
        {
            if (_cancellationMap.ContainsKey(task.Name))
                throw new InvalidOperationException($"Task '{task.Name}' is already running.");

            var cts = new CancellationTokenSource();
            _cancellationMap[task.Name] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await task.RunAsync(cts.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {task.Name}: {ex.Message}");
                }
                finally
                {
                    StopTask(task.Name); // タスクが終了したら自動的に削除
                }
            });
        }
    }

    public void StopTask(string name)
    {
        lock (_lock)
        {
            if (_cancellationMap.TryGetValue(name, out var cts))
            {
                cts.Cancel();
                _cancellationMap.Remove(name);
            }
        }
    }

    public void StopAll()
    {
        lock (_lock)
        {
            foreach (var cts in _cancellationMap.Values)
                cts.Cancel();

            _cancellationMap.Clear();
        }
    }

    public bool IsRunning(string name)
    {
        lock (_lock)
        {
            return _cancellationMap.ContainsKey(name);
        }
    }
}


---

3. ✅ サンプル：ログを出力する定期タスク

public class HeartbeatTask : ScheduledTaskBase
{
    public HeartbeatTask(string name, TimeSpan interval)
        : base(name, interval) { }

    protected override Task ExecuteOnceAsync()
    {
        ReportProgress($"[{DateTime.Now}] {Name}: ハートビート送信");
        return Task.CompletedTask;
    }
}


---

4. ✅ 実行例

var timerService = new TimerService();
var heartbeat = new HeartbeatTask("Heartbeat", TimeSpan.FromSeconds(10));

timerService.StartTask(heartbeat);

// 数十秒後に停止したい場合
// timerService.StopTask("Heartbeat");


---

🧩 拡張案


---

必要なら cron 式によるスケジューリング や、UIでON/OFF切り替えする設定管理、ログファイルへの出力 にも拡張可能です。
他にも組み込みたい機能があれば教えてください！





了解です！
それでは、定期実行の動作制御を柔軟に切り替えるために、OverrunStrategy を導入した形で ScheduledTaskBase を拡張します。


---

✅ OverrunStrategy とは？

定期実行タスクで、処理がスケジュール時間を過ぎて終了した場合の挙動を切り替える戦略です。


---

1. 🔧 列挙型 OverrunStrategy

public enum OverrunStrategy
{
    FixedInterval,    // 固定間隔（前回終了から一定間隔）
    CatchUp,          // スケジュール基準で即時実行
    Skip              // スケジュール基準、次の時刻までスキップ
}


---

2. 🔧 ScheduledTaskBase に統合

public abstract class ScheduledTaskBase : TaskBase
{
    public TimeSpan Interval { get; }
    public OverrunStrategy OverrunStrategy { get; }

    protected ScheduledTaskBase(string name, TimeSpan interval, OverrunStrategy overrunStrategy = OverrunStrategy.FixedInterval)
        : base(name)
    {
        Interval = interval;
        OverrunStrategy = overrunStrategy;
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
                        // スキップ（遅れているので次の1回分飛ばす）
                        nextRunTime = DateTime.Now + Interval;
                        ReportProgress("スケジュール遅延によりスキップ");
                    }
                    break;
            }
        }
    }
}


---

3. ✅ 使用例

var heartbeat = new HeartbeatTask(
    name: "Heartbeat",
    interval: TimeSpan.FromSeconds(5),
    overrunStrategy: OverrunStrategy.Skip
);

timerService.StartTask(heartbeat);


---

✅ 各戦略の比較


---

🧩 拡張案


---

OverrunStrategy により柔軟な制御ができるようになりました。
必要なら ログ出力付きテンプレート や 構成ファイルから自動生成 も追加できます。

もっと進めますか？

