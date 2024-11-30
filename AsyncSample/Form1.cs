using System.Diagnostics;
using System.Timers;
using System.Windows.Forms;

namespace AsyncSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnAwait_Click(object sender, EventArgs e)
        {
            // 呼び出し元のスレッド（UIスレッド）
            Log($"btnAwait_Click (Before await)");

            // 非同期処理をawait
            await PerformAsyncTask();

            // await後のスレッド（UIスレッドに戻る）
            Log($"btnAwait_Click (After await)");
        }

        private void btnWait_Click(object sender, EventArgs e)
        {
            // 呼び出し元のスレッド（UIスレッド）
            Log($"btnWait_Click (Before Wait)");

            // 非同期処理を同期的に待機
            PerformAsyncTask().Wait();

            // Wait後のスレッド（UIスレッド）
            Log($"btnWait_Click (After Wait)");
        }

        private async Task PerformAsyncTask()
        {
            // 非同期処理の開始
            Log("PerformAsyncTask (Start)");

            // 1秒間の非同期処理
            await Task.Run(() =>
            {
                Log("PerformAsyncTask (Inside Task.Run - Background Thread)");
                Thread.Sleep(1000); // 1秒待機
            });

            // 非同期処理完了後（await後）
            Log("PerformAsyncTask (End - Back to UI Thread)");
        }

        private async void btnAsyncVoid_ClickAsync(object sender, EventArgs e)
        {
            Log($"btnAsyncVoid_Click (Before await)");

            // 非同期処理を実行
            await PerformAsyncMethod();

            Log($"btnAsyncVoid_Click (After await)");
        }

        private async Task PerformAsyncMethod()
        {
            Log("PerformAsyncMethod (Start)");
            await Task.Delay(1000); // 1秒待機
            Log("PerformAsyncMethod (End)");
        }

        private void btnContinueWith_Click(object sender, EventArgs e)
        {
            Log("btnContinueWith_Click (Start)");

            // タスクを開始し、完了後にContinueWithで処理を続ける
            Task.Run(() =>
            {
                Log("Task.Run (Inside Background Task)");
                Thread.Sleep(1000); // 1秒待機
                return "Task completed";
            })
            .ContinueWith(task =>
            {
                Log("ContinueWith (On Completion)");
                Log($"Task Result: {task.Result}");
            }); // UIスレッドで続行

            Log("btnContinueWith_Click (End)");
        }

        private void btnException_Click(object sender, EventArgs e)
        {
            Log("btnException_Click (Start)");

            // タスク内で例外を発生させる
            Task.Run(() =>
            {
                Log("Task.Run (Inside Background Task - Before Exception)");
                throw new InvalidOperationException("Something went wrong in the task");
            })
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Log("ContinueWith (On Exception)");
                    Log($"Exception: {task.Exception?.Flatten().InnerException?.Message}");
                }
            }); // UIスレッドで続行

            Log("btnException_Click (End)");
        }

        private void Log(string message)
        {
            // 現在のスレッドIDをログに出力
            string log = $"{DateTime.Now:HH:mm:ss.fff} [Thread: {Thread.CurrentThread.ManagedThreadId}] {message}";
            Debug.WriteLine(log);
            // UIスレッド上でlistBox1を更新
            //if (listBox1.InvokeRequired)
            //{
            //    // UIスレッド以外の場合、InvokeでUIスレッドに切り替える
            //    listBox1.Invoke(new Action(() => listBox1.Items.Add(log)));
            //}
            //else
            //{
            //    // UIスレッド上の場合、そのまま更新
            //    listBox1.Items.Add(log);
            //}
        }
    }
}
