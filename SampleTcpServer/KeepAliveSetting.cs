using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpServer
{
    public class KeepaliveSetting
    {
        private int _tcpKeepAliveInterval = 2;
        private int _tcpKeepAliveTime = 2;
        private int _tcpKeepAliveRetryCount = 3;
        public bool EnableTcpKeepAlives = false;

        public int TcpKeepAliveInterval
        {
            get
            {
                return _tcpKeepAliveInterval;
            }
            set
            {
                if (value < 1) throw new ArgumentException("TcpKeepAliveInterval must be greater than zero.");
                _tcpKeepAliveInterval = value;
            }
        }

        public int TcpKeepAliveTime
        {
            get
            {
                return _tcpKeepAliveTime;
            }
            set
            {
                if (value < 1) throw new ArgumentException("TcpKeepAliveTime must be greater than zero.");
                _tcpKeepAliveTime = value;
            }
        }

        public int TcpKeepAliveRetryCount
        {
            get
            {
                return _tcpKeepAliveRetryCount;
            }
            set
            {
                if (value < 1) throw new ArgumentException("TcpKeepAliveRetryCount must be greater than zero.");
                _tcpKeepAliveRetryCount = value;
            }
        }

        internal int TcpKeepAliveIntervalMilliseconds => TcpKeepAliveInterval * 1000;

        internal int TcpKeepAliveTimeMilliseconds => TcpKeepAliveTime * 1000;

        public KeepaliveSetting()
        {

        }

    }
}
