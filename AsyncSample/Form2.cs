using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncSample
{
    public partial class Form2 : Form
    {
        ScheduledTaskService service;

        public Form2()
        {
            InitializeComponent();
            service = new ScheduledTaskService(true, new DebugProgress());
            service.TaskStarted += (s, name) => Debug.Print($"[EVENT] タスク開始: {name}");
            service.TaskCompleted += (s, name) => Debug.Print($"[EVENT] タスク完了: {name}");
            service.TaskFailed += (s, e) => Debug.Print($"[ERROR] タスク失敗: {e.Name} - {e.Exception.Message}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            service.StopAll();
        }

        public class DebugProgress : IProgress<TaskSnapshot>
        {
            public void Report(TaskSnapshot value)
            {
                Debug.Print($"[{value.Name}]{value.Message}");
            }
        }

        public class TestTask : ScheduledTaskBase
        {
            private readonly string _message;
            private readonly TimeSpan _workDuration;
            private readonly int _failOnRun;

            public TestTask(string name, TimeSpan interval, OverrunStrategy strategy, string message, TimeSpan workDuration, int failOnRun = -1)
                : base(name, interval, strategy)
            {
                _message = message;
                _workDuration = workDuration;
                _failOnRun = failOnRun;
            }

            protected override async Task ExecuteOnceAsync()
            {
                RunCount++;
                if (_failOnRun > 0 && RunCount == _failOnRun)
                {
                    throw new InvalidOperationException($"故意の例外発生: {_message}");
                }

                Debug.Print($"[{DateTime.Now:HH:mm:ss}] {_message} 実行開始 (RunCount: {RunCount})");
                await Task.Delay(_workDuration, CancellationToken);
                Debug.Print($"[{DateTime.Now:HH:mm:ss}] {_message} 実行終了");
            }
        }

        private void buttonFixedInterval_Click(object sender, EventArgs e)
        {
            var task = new TestTask(
                name: "FixedTask",
                interval: TimeSpan.FromSeconds(5),
                strategy: OverrunStrategy.FixedInterval,
                message: "Fixed間隔タスク",
                workDuration: TimeSpan.FromSeconds(2)
            );
            service.StartTask(task);
        }

        private void buttonCatchUp_Click(object sender, EventArgs e)
        {
            var task = new TestTask(
                name: "CatchUpTask",
                interval: TimeSpan.FromSeconds(5),
                strategy: OverrunStrategy.CatchUp,
                message: "CatchUpタスク",
                workDuration: TimeSpan.FromSeconds(8)  // オーバーランする
            );
            service.StartTask(task);
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            var task = new TestTask(
                name: "SkipTask",
                interval: TimeSpan.FromSeconds(5),
                strategy: OverrunStrategy.Skip,
                message: "Skipタスク",
                workDuration: TimeSpan.FromSeconds(8)
            );
            service.StartTask(task);
        }

        private void buttonFail_Click(object sender, EventArgs e)
        {
            var task = new TestTask(
                name: "FailTask",
                interval: TimeSpan.FromSeconds(5),
                strategy: OverrunStrategy.FixedInterval,
                message: "例外タスク",
                workDuration: TimeSpan.FromSeconds(2),
                failOnRun: 3
            );
            service.StartTask(task);
        }

        private void buttonStoppable_Click(object sender, EventArgs e)
        {
            var task = new TestTask(
                name: "StoppableTask",
                interval: TimeSpan.FromSeconds(2),
                strategy: OverrunStrategy.FixedInterval,
                message: "停止テストタスク",
                workDuration: TimeSpan.FromSeconds(1)
            );
            service.StartTask(task);
        }

        private void buttonStopStoppable_Click(object sender, EventArgs e)
        {
            service.StopTask("StoppableTask");
            Console.WriteLine("タスクを停止しました");
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            var task = new TestTask(
                name: "FixedTask",
                interval: TimeSpan.FromSeconds(5),
                strategy: OverrunStrategy.FixedInterval,
                message: "Fixed間隔タスク",
                workDuration: TimeSpan.FromSeconds(2)
            );
            service.StartTask(task);

            //var task2 = new TestTask(
            //    name: "CatchUpTask",
            //    interval: TimeSpan.FromSeconds(5),
            //    strategy: OverrunStrategy.CatchUp,
            //    message: "CatchUpタスク",
            //    workDuration: TimeSpan.FromSeconds(8)  // オーバーランする
            //);
            //service.StartTask(task2);

            //var task3 = new TestTask(
            //    name: "SkipTask",
            //    interval: TimeSpan.FromSeconds(5),
            //    strategy: OverrunStrategy.Skip,
            //    message: "Skipタスク",
            //    workDuration: TimeSpan.FromSeconds(8)
            //);
            //service.StartTask(task3);
        }
    }
}
