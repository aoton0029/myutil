using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tcp.Simple
{
    public class Client
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Encoding _encoding = Encoding.ASCII;
        private string _delimiter = "\r\n";
        private Mutex _mutex = new Mutex();

        public bool Open(string hostname, int port)
        {
            try
            {
                _client = new TcpClient(hostname, port);
                _stream = _client.GetStream();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening connection: {ex.Message}");
                return false;
            }
        }

        public void Close()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing connection: {ex.Message}");
            }
        }

        public CommResult SendReceive(string message)
        {
            if (_stream == null || !_client.Connected)
            {
                return new CommResult(null, false, "Connection is not open.");
            }

            _mutex.WaitOne();
            try
            {
                // Sending data
                byte[] dataToSend = _encoding.GetBytes(message);
                _stream.Write(dataToSend, 0, dataToSend.Length);

                // Receiving data
                StringBuilder receivedData = new StringBuilder();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while (true)
                {
                    // Read from the stream
                    bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        // Append the data to the receivedData string builder
                        receivedData.Append(_encoding.GetString(buffer, 0, bytesRead));

                        // Check if the delimiter is found in the received data
                        if (receivedData.ToString().Contains(_delimiter))
                        {
                            break;
                        }
                    }
                }

                // Convert the received data to a string and remove the delimiter
                string fullMessage = receivedData.ToString();
                int delimiterIndex = fullMessage.IndexOf(_delimiter);

                if (delimiterIndex >= 0)
                {
                    fullMessage = fullMessage.Substring(0, delimiterIndex);
                }

                return CommResult.Success(fullMessage);
            }
            catch (Exception ex)
            {
                return CommResult.Exeption(ex.Message);
            }
            finally
            {
                _mutex.ReleaseMutex(); // Mutexを解放
            }
        }

    }

}
