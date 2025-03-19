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

        // USB�f�o�C�X�̑}���E�폜���Ď�
        private void InitializeUSBWatcher()
        {
            watcher = new ManagementEventWatcher();
            watcher.EventArrived += new EventArrivedEventHandler(OnUSBChanged);
            watcher.Query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
            watcher.Start();
        }

        // USB�f�o�C�X�̑}���E�폜�����o�����Ƃ��̏���
        private void OnUSBChanged(object sender, EventArrivedEventArgs e)
        {
            Invoke((MethodInvoker)LoadRemovableDriveAudioFiles);
        }

        // �����[�o�u���X�g���[�W�̉����t�@�C����ǂݍ���
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
                MessageBox.Show("�����[�o�u���X�g���[�W���ڑ�����Ă��܂���B", "�ʒm", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (listBoxFiles.SelectedItem == null)
            {
                MessageBox.Show("�Đ�����t�@�C����I�����Ă��������B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedFile = listBoxFiles.SelectedItem.ToString();
            PlayAudio(selectedFile);
        }

        // �����t�@�C�����Đ����郁�\�b�h
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
                MessageBox.Show($"�����t�@�C���̍Đ��Ɏ��s���܂���: {ex.Message}", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // �Đ����~����
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
