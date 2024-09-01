using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpServer
{
    internal class ClientMetadata : IDisposable
    {
        internal TcpClient Client => _tcpClient;
        internal NetworkStream NetworkStream => _networkStream;
        internal string IpPort => _ipPort;
        internal SemaphoreSlim SendLock = new SemaphoreSlim(1, 1);
        internal SemaphoreSlim ReceiveLock = new SemaphoreSlim(1, 1);
        internal CancellationTokenSource TokenSource { get; set; }
        internal CancellationToken Token { get; set; }

        private TcpClient _tcpClient = null;
        private NetworkStream _networkStream = null;
        private string _ipPort = null;

        internal ClientMetadata(TcpClient tcp)
        {
            if (tcp == null) throw new ArgumentNullException(nameof(tcp));

            _tcpClient = tcp;
            _networkStream = tcp.GetStream();
            _ipPort = tcp.Client.RemoteEndPoint.ToString();
            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
        }

        public void Dispose()
        {
            if (TokenSource != null)
            {
                if (!TokenSource.IsCancellationRequested)
                {
                    TokenSource.Cancel();
                    TokenSource.Dispose();
                }
            }

            if (_networkStream != null)
            {
                _networkStream.Close();
            }

            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient.Dispose();
            }

            SendLock.Dispose();
            ReceiveLock.Dispose();
        }
    }
}
