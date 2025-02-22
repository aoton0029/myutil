using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SampleTcpClient
{
    public class SerialConnection : IConnection
    {
        private readonly SerialPort _serialPort;
        private readonly int _timeout;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialConnection(string portName, int baudRate = 9600, int timeout = 5000,
                                Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                Encoding = Encoding.UTF8,
                ReadTimeout = timeout,
                WriteTimeout = timeout
            };
            _timeout = timeout;
        }

        public void Connect()
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new InvalidOperationException($"Access to {_serialPort.PortName} denied.");
            }
            catch (IOException)
            {
                throw new InvalidOperationException($"Failed to open serial port {_serialPort.PortName}.");
            }
        }

        public void Disconnect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public void Send(string command)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected.");

            try
            {
                _serialPort.WriteLine(command);
            }
            catch (TimeoutException)
            {
                throw new TimeoutException("Serial write timeout.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send data: {ex.Message}");
            }
        }

        public string Read()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected.");

            try
            {
                return _serialPort.ReadLine();
            }
            catch (TimeoutException)
            {
                throw new TimeoutException("Serial read timeout.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read data: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Disconnect();
            _serialPort.Dispose();
        }
    }
}
