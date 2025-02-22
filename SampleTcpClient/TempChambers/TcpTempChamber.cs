using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpClient.TempChambers
{
    public class TcpTempChamber : TempChamber
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _host;
        private readonly int _port;

        public bool IsConnected => _client?.Connected ?? false;

        public override void Open(out string errMsg)
        {
            throw new NotImplementedException();
        }

        public override void Close(out string errMsg)
        {
            throw new NotImplementedException();
        }

        public override void GetMon(out string errMsg)
        {
            throw new NotImplementedException();
        }

        public override void GetMode(out string errMsg)
        {
            throw new NotImplementedException();
        }

        public override void GetTemp(out string errMsg)
        {
            throw new NotImplementedException();
        }

        public override void GetAlart(out string errMsg)
        {
            throw new NotImplementedException();
        }

        public override void SetTemp(out string errMsg)
        {
            throw new NotImplementedException();
        }

        public override void SetMode(out string errMsg)
        {
            throw new NotImplementedException();
        }
    }
}
