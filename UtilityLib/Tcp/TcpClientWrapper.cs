using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tcp
{
    public class TcpClientWrapper
    {
        private TcpClient client;
        private NetworkStream stream;

        public TcpClientWrapper(string host, int port)
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
        }

        public async Task SendAsync(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task<string> ReceiveAsync()
        {
            var buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public async Task<string> SendAndReceiveAsync(string message)
        {
            await SendAsync(message);
            return await ReceiveAsync();
        }

        public void Close()
        {
            stream.Close();
            client.Close();
        }
    }
}
