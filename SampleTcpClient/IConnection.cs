using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpClient
{
    public interface IConnection : IDisposable
    {
        bool IsConnected { get; }
        void Connect();
        void Disconnect();
        void Send(string command);
        string Read();
    }
}
