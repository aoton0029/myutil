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
}
