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

