using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpServer
{
    public class Statistic
    {
        private DateTime _startTime = DateTime.Now.ToUniversalTime();
        private long _receivedBytes = 0;
        private long _sentBytes = 0;

        public DateTime StartTime_startTime => _startTime;
        public TimeSpan UpTime => DateTime.Now.ToUniversalTime() - _startTime;
        public long ReceivedBytes { get => _receivedBytes; internal set=> _receivedBytes = value; }
        public long SentBytes { get => _sentBytes; internal set => _sentBytes = value; }

        public Statistic()
        {

        }

        public override string ToString()
        {
            string ret =
                "--- Statistics ---" + Environment.NewLine +
                "    Started        : " + _startTime.ToString() + Environment.NewLine +
                "    Uptime         : " + UpTime.ToString() + Environment.NewLine +
                "    Received bytes : " + ReceivedBytes + Environment.NewLine +
                "    Sent bytes     : " + SentBytes + Environment.NewLine;
            return ret;
        }

        public void Reset()
        {
            _receivedBytes = 0;
            _sentBytes = 0;
        }
    }
}
