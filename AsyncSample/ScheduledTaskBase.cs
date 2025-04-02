using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public abstract class ScheduledTaskBase : TaskBase
    {
        public TimeSpan Interval { get; }
        public OverrunStrategy OverrunStrategy { get; }
        public int? MaxSkipCount { get; }
        public int RunCount { get; set; }
        public DateTime? LastRun { get; set; }

        private int _skipCount = 0;
        private readonly Func<SkipContext, bool> _shouldSkip;

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

                if (now < nextRunTime)
                {
                    await Task.Delay(nextRunTime - now, CancellationToken);
                }

                CancellationToken.ThrowIfCancellationRequested();

                var actualStart = DateTime.Now;

                var skipContext = new SkipContext
                {
                    ScheduledTime = nextRunTime,
                    ActualStartTime = actualStart,
                    SkipCount = _skipCount
                };

                if (_shouldSkip(skipContext))
                {
                    _skipCount++;
                    ReportProgress($"スケジュール遅延によりスキップ {_skipCount} 回");

                    OnSkipped(skipContext);

                    if (MaxSkipCount.HasValue && _skipCount > MaxSkipCount.Value)
                    {
                        var msg = $"スキップ回数が上限 {MaxSkipCount} を超えました";
                        ReportProgress(msg);
                        throw new Exception(msg);
                    }

                    nextRunTime = DateTime.Now + Interval;
                    continue;
                }

                try
                {
                    OnBeforeExecute();

                    CancellationToken.ThrowIfCancellationRequested();
                    await ExecuteOnceAsync();

                    OnAfterExecute();
                }
                catch (OperationCanceledException)
                {
                    // 通常のキャンセル → そのまま抜ける
                    break;
                }
                catch (Exception ex)
                {
                    ReportProgress($"エラー発生: {ex.Message}");
                    OnError(ex);
                    throw;
                }

                RunCount++;
                LastRun = DateTime.Now;
                _skipCount = 0;

                nextRunTime += Interval;
            }
        }

        protected virtual void OnBeforeExecute() { }
        protected virtual void OnAfterExecute() { }
        protected virtual void OnSkipped(SkipContext ctx) { }
        protected virtual void OnError(Exception ex) { }

        private static bool DefaultSkipStrategy(SkipContext ctx)
        {
            return ctx.ActualStartTime - ctx.ScheduledTime > ctx.ActualStartTime - ctx.ScheduledTime; // 必ずスキップ
        }
    }

    public class SkipContext
    {
        public DateTime ScheduledTime { get; init; }
        public DateTime ActualStartTime { get; init; }
        public int SkipCount { get; init; }
    }
}
