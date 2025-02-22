using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpClient
{
    public abstract class TempChamber
    {
        public enum Mode
        {
            SET,
            MONITOR
        }

        protected readonly int _timeout = 5000;

        public TempChamber()
        {

        }

        public abstract void Open(out string errMsg);
        public abstract void Close(out string errMsg);

        public abstract void GetMon(out string errMsg);
        public abstract void GetMode(out string errMsg);
        public abstract void GetTemp(out string errMsg);
        public abstract void GetAlart(out string errMsg);
        public abstract void SetTemp(out string errMsg);
        public abstract void SetMode(out string errMsg);

    }
}
