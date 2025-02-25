using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SampleTcpServer.Simulators
{
    class DeviceServer
    {
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly TcpListener _listener;
        private readonly IDevice _device;
        private readonly IDeviceProtocol _protocol;
        private readonly AsyncLogProcessor _logProcessor;

        public DeviceServer(string ipAddress, int port, IDevice device, IDeviceProtocol protocol, AsyncLogProcessor logProcessor)
        {
            _ipAddress = ipAddress;
            _port = port;
            _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            _device = device;
            _protocol = protocol;
            _logProcessor = logProcessor;
        }

        public async Task StartAsync()
        {
            try
            {
                _listener.Start();
                _logProcessor.Log($"[{_device.Name}] サーバー起動 {_ipAddress}:{_port}");

                while (true)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _logProcessor.Log($"[{_device.Name}] クライアント接続");

                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                _logProcessor.Log($"[{_device.Name}] サーバーエラー: {ex.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (IsClientConnected(client))
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException)
                    {
                        _logProcessor.Log($"[{_device.Name}] クライアント通信エラー (切断された可能性): {ex.Message}");
                        break;
                    }

                    if (bytesRead == 0) break; // クライアントが切断

                    string receivedText = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    string decodedCommand = _protocol.Decode(receivedText);
                    _logProcessor.Log($"[{_device.Name}] 受信: {decodedCommand}");

                    string response = _device.ProcessCommand(decodedCommand);
                    string encodedResponse = _protocol.Encode(response);

                    try
                    {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(encodedResponse + "\n");
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException)
                    {
                        _logProcessor.Log($"[{_device.Name}] クライアント送信エラー (切断された可能性): {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logProcessor.Log($"[{_device.Name}] クライアント処理エラー: {ex.Message}");
            }
            finally
            {
                _logProcessor.Log($"[{_device.Name}] クライアント切断");
                client.Close();
            }
        }

        private bool IsClientConnected(TcpClient client)
        {
            try
            {
                if (client == null || !client.Connected) return false;

                // Poll(0, SelectMode.SelectRead) を使い、接続が維持されているか確認
                var socket = client.Client;
                if (socket.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buffer = new byte[1];
                    if (socket.Receive(buffer, SocketFlags.Peek) == 0)
                    {
                        // クライアントが切断された場合
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        public void Stop()
        {
            try
            {
                _listener.Stop();
                _logProcessor.Log($"[{_device.Name}] サーバー停止");
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }
    }
}
