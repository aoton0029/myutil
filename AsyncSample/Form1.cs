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
            // �Ăяo�����̃X���b�h�iUI�X���b�h�j
            Log($"btnAwait_Click (Before await)");

            // �񓯊�������await
            await PerformAsyncTask();

            // await��̃X���b�h�iUI�X���b�h�ɖ߂�j
            Log($"btnAwait_Click (After await)");
        }

        private void btnWait_Click(object sender, EventArgs e)
        {
            // �Ăяo�����̃X���b�h�iUI�X���b�h�j
            Log($"btnWait_Click (Before Wait)");

            // �񓯊������𓯊��I�ɑҋ@
            PerformAsyncTask().Wait();

            // Wait��̃X���b�h�iUI�X���b�h�j
            Log($"btnWait_Click (After Wait)");
        }

        private async Task PerformAsyncTask()
        {
            // �񓯊������̊J�n
            Log("PerformAsyncTask (Start)");

            // 1�b�Ԃ̔񓯊�����
            await Task.Run(() =>
            {
                Log("PerformAsyncTask (Inside Task.Run - Background Thread)");
                Thread.Sleep(1000); // 1�b�ҋ@
            });

            // �񓯊�����������iawait��j
            Log("PerformAsyncTask (End - Back to UI Thread)");
        }

        private async void btnAsyncVoid_ClickAsync(object sender, EventArgs e)
        {
            Log($"btnAsyncVoid_Click (Before await)");

            // �񓯊����������s
            await PerformAsyncMethod();

            Log($"btnAsyncVoid_Click (After await)");
        }

        private async Task PerformAsyncMethod()
        {
            Log("PerformAsyncMethod (Start)");
            await Task.Delay(1000); // 1�b�ҋ@
            Log("PerformAsyncMethod (End)");
        }

        private void btnContinueWith_Click(object sender, EventArgs e)
        {
            Log("btnContinueWith_Click (Start)");

            // �^�X�N���J�n���A�������ContinueWith�ŏ����𑱂���
            Task.Run(() =>
            {
                Log("Task.Run (Inside Background Task)");
                Thread.Sleep(1000); // 1�b�ҋ@
                return "Task completed";
            })
            .ContinueWith(task =>
            {
                Log("ContinueWith (On Completion)");
                Log($"Task Result: {task.Result}");
            }); // UI�X���b�h�ő��s

            Log("btnContinueWith_Click (End)");
        }

        private void btnException_Click(object sender, EventArgs e)
        {
            Log("btnException_Click (Start)");

            // �^�X�N���ŗ�O�𔭐�������
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
            }); // UI�X���b�h�ő��s

            Log("btnException_Click (End)");
        }

        private void Log(string message)
        {
            // ���݂̃X���b�hID�����O�ɏo��
            string log = $"{DateTime.Now:HH:mm:ss.fff} [Thread: {Thread.CurrentThread.ManagedThreadId}] {message}";
            Debug.WriteLine(log);
            // UI�X���b�h���listBox1���X�V
            //if (listBox1.InvokeRequired)
            //{
            //    // UI�X���b�h�ȊO�̏ꍇ�AInvoke��UI�X���b�h�ɐ؂�ւ���
            //    listBox1.Invoke(new Action(() => listBox1.Items.Add(log)));
            //}
            //else
            //{
            //    // UI�X���b�h��̏ꍇ�A���̂܂܍X�V
            //    listBox1.Items.Add(log);
            //}
        }
    }
}
