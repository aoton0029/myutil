using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpClient.TempChambers
{
    internal class SerialTempChamber: TempChamber
    {
        private readonly SerialPort _serialPort;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialTempChamber(string portName, int baudRate = 9600, int timeout = 5000,
                                Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                Encoding = Encoding.UTF8,
                ReadTimeout = timeout,
                WriteTimeout = timeout
            };
        }

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
