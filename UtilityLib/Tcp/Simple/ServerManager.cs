using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tcp.Simple
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string ReceivedData { get; set; }
        public string ResponseData { get; set; }
    }

    public class ServerManager
    {
        private Server _tcpServer;
        private List<LogEntry> _logs;
        private SCPICommandManager _commandManager;
        public Func<string, string> process;

        public ServerManager(string ipAddress, int port)
        {
            _tcpServer = new Server(ipAddress, port);
            _logs = new List<LogEntry>();

            // 応答メソッドを設定
            _tcpServer.ResponseMethod = ProcessRequest;
        }

        public ServerManager(string ipAddress, int port, SCPICommandManager commMng) : this(ipAddress, port)
        {
            _commandManager = commMng;
        }

        public void Start()
        {
            _tcpServer.Start();
        }

        public void Stop()
        {
            _tcpServer.Stop();
        }

        // ログを取得するメソッド
        public IEnumerable<LogEntry> GetLogs()
        {
            return _logs.AsReadOnly();
        }

        // 受信データに基づいて適切な応答を返すメソッド
        private string ProcessRequest(string receivedData)
        {
            string responseData = process(receivedData);

            // ログにエントリを追加
            _logs.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                ReceivedData = receivedData,
                ResponseData = responseData
            });

            return responseData;
        }
    }

}
