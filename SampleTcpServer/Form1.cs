using SampleTcpServer.Simples;
using SampleTcpServer.Simulators;
using System.Diagnostics;

namespace SampleTcpServer
{
    public partial class Form1 : Form
    {
        ServerManager manager;
        AsyncLogProcessor logProcessor;

        public Form1()
        {
            InitializeComponent();
            logProcessor = new();
            manager = new(logProcessor);
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() => manager.StartAll());

                Debug.Print("サーバーが起動しました。");
            }
            catch (Exception ex)
            {
                Debug.Print($"{ex}");
            }
        }

        private async void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                await manager.StopAllAsync();
            }
            catch (Exception ex)
            {
                Debug.Print($"{ex}");
            }
        }
    }
}
