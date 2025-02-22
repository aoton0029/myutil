using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpClient
{
    public class TcpConnection : IConnection
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _host;
        private readonly int _port;
        private readonly int _timeout;

        public bool IsConnected => _client?.Connected ?? false;

        public TcpConnection(string host, int port, int timeout = 5000)
        {
            _host = host;
            _port = port;
            _timeout = timeout;
        }

        public void Connect()
        {
            try
            {
                _client = new TcpClient
                {
                    ReceiveTimeout = _timeout,
                    SendTimeout = _timeout
                };

                var task = _client.ConnectAsync(_host, _port);
                if (!task.Wait(_timeout))
                {
                    throw new TimeoutException("TCP connection timeout.");
                }

                _stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to connect to {_host}:{_port}. {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
        }

        public void Send(string command)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected.");
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(command + "\n");
                _stream.Write(data, 0, data.Length);
            }
            catch (IOException)
            {
                throw new TimeoutException("TCP send timeout.");
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
                var task = Task.Run(() =>
                {
                    using var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);
                    return reader.ReadLine();
                });

                if (!task.Wait(_timeout))
                {
                    throw new TimeoutException("TCP read timeout.");
                }

                return task.Result ?? throw new IOException("No response received.");
            }
            catch (IOException)
            {
                throw new TimeoutException("TCP read timeout.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read data: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
