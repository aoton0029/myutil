using SampleTcpServer.Simples;

namespace SampleTcpServer
{
    public partial class Form1 : Form
    {
        Server _server; 

        public Form1()
        {
            InitializeComponent();

            _server = new Server("192.168.1.10", 8080);
            _server.Start();
        }
    }
}
