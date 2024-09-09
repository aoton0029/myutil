using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    public class a 
    {
        public void Main()
        {
            var tcpClient = new ObservableTcpClient();

            // サーバに接続し、メッセージを送信して、応答を受信
            var tcpStream = tcpClient.Connect("localhost", 8080)
                                     .SelectMany(_ => tcpClient.SendAndReceive("Hello, Server!"))
                                     .SelectMany(_ => tcpClient.SendAndReceive("Another Message"))
                                     .SelectMany(_ => tcpClient.Disconnect());

            // ストリームに購読
            var subscription = tcpStream.Subscribe(
                onNext: result => Console.WriteLine($"Sent: {result.sent}, Received: {result.received}"),
                onError: ex => Console.WriteLine($"Error: {ex.Message}"),
                onCompleted: () => Console.WriteLine("Communication completed")
            );

            Console.ReadLine(); // 実行の維持
        }
    }


    public class ObservableTcpClient : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream _networkStream;

        public ObservableTcpClient()
        {
            _tcpClient = new TcpClient();
        }

        public IObservable<bool> Connect(string host, int port)
        {
            return new AnonymousObservable<bool>(async observer =>
            {
                try
                {
                    await _tcpClient.ConnectAsync(host, port);
                    _networkStream = _tcpClient.GetStream();
                    observer.OnNext(true);
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
                return Disposable.Empty;
            });
        }

        public IObservable<int> Send(string message)
        {
            return new AnonymousObservable<int>(async observer =>
            {
                try
                {
                    if (_networkStream == null)
                        throw new InvalidOperationException("Not connected to the server.");

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await _networkStream.WriteAsync(data, 0, data.Length);
                    observer.OnNext(data.Length);
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
                return Disposable.Empty;
            });
        }

        public IObservable<string> Receive()
        {
            return new AnonymousObservable<string>(async observer =>
            {
                try
                {
                    if (_networkStream == null)
                        throw new InvalidOperationException("Not connected to the server.");

                    byte[] buffer = new byte[1024];
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    observer.OnNext(receivedData);
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
                return Disposable.Empty;
            });
        }

        public IObservable<(string sent, string received)> SendAndReceive(string message)
        {
            return new AnonymousObservable<(string sent, string received)>(observer =>
            {
                var sendObservable = Send(message);
                var receiveObservable = Receive();

                return sendObservable.SelectMany(_ => receiveObservable)
                                     .Subscribe(
                                         received => observer.OnNext((message, received)),
                                         observer.OnError,
                                         observer.OnCompleted
                                     );
            });
        }

        public IObservable<bool> Disconnect()
        {
            return new AnonymousObservable<bool>(observer =>
            {
                try
                {
                    _networkStream?.Close();
                    _tcpClient.Close();
                    observer.OnNext(true);
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
                return Disposable.Empty;
            });
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
        }

        // 受信処理にタイムアウト機能を追加
        public IObservable<string> ReceiveWithTimeout(TimeSpan timeout)
        {
            return new AnonymousObservable<string>(async observer =>
            {
                try
                {
                    if (_networkStream == null)
                        throw new InvalidOperationException("Not connected to the server.");

                    byte[] buffer = new byte[1024];
                    var cts = new CancellationTokenSource(timeout);

                    var bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    observer.OnNext(receivedData);
                    observer.OnCompleted();
                }
                catch (TaskCanceledException)
                {
                    observer.OnError(new TimeoutException("Receive operation timed out."));
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return Disposable.Empty;
            });
        }
    }
}
