using System.Management;
using System.Media;
using System.Numerics;

namespace Sample
{
    public partial class Form1 : Form
    {
        private ManagementEventWatcher watcher;
        private string removableDrive = "";

        public Form1()
        {
            InitializeComponent();
            InitializeUSBWatcher();
            LoadRemovableDriveAudioFiles();
        }

        // USBデバイスの挿入・削除を監視
        private void InitializeUSBWatcher()
        {
            watcher = new ManagementEventWatcher();
            watcher.EventArrived += new EventArrivedEventHandler(OnUSBChanged);
            watcher.Query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
            watcher.Start();
        }

        // USBデバイスの挿入・削除を検出したときの処理
        private void OnUSBChanged(object sender, EventArrivedEventArgs e)
        {
            Invoke((MethodInvoker)LoadRemovableDriveAudioFiles);
        }

        // リムーバブルストレージの音声ファイルを読み込む
        private void LoadRemovableDriveAudioFiles()
        {
            listBoxFiles.Items.Clear();
            removableDrive = "";

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable && d.IsReady))
            {
                removableDrive = drive.RootDirectory.FullName;
                string[] audioExtensions = { ".mp3", ".wav" };
                var files = Directory.GetFiles(removableDrive, "*.*", SearchOption.AllDirectories)
                                     .Where(f => audioExtensions.Contains(Path.GetExtension(f).ToLower()))
                                     .ToList();

                listBoxFiles.Items.AddRange(files.ToArray());
            }

            if (string.IsNullOrEmpty(removableDrive))
            {
                MessageBox.Show("リムーバブルストレージが接続されていません。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (listBoxFiles.SelectedItem == null)
            {
                MessageBox.Show("再生するファイルを選択してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedFile = listBoxFiles.SelectedItem.ToString();
            PlayAudio(selectedFile);
        }

        // 音声ファイルを再生するメソッド
        private void PlayAudio(string filePath)
        {
            StopAudio();

            try
            {
                axWindowsMediaPlayer1.URL = filePath;
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"音声ファイルの再生に失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 再生を停止する
        private void StopAudio()
        {
            axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAudio();
            watcher.Stop();
            watcher.Dispose();
            axWindowsMediaPlayer1.Dispose();
        }
    }
}
