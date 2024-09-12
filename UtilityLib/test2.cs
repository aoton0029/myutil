using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TcpClientExtensions
{
    // TCPクライアントでデータを送信するメソッド
    public static IObservable<int> SendAsync(this TcpClient client, string message)
    {
        return Observable.Create<int>(async observer =>
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                NetworkStream stream = client.GetStream();
                
                // 非同期でデータを送信
                await stream.WriteAsync(data, 0, data.Length);
                observer.OnNext(data.Length);
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return client;
        });
    }

    // TCPクライアントでデータを受信するメソッド
    public static IObservable<string> ReceiveAsync(this TcpClient client)
    {
        return Observable.Create<string>(async observer =>
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                // 非同期でデータを受信
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    observer.OnNext(message);
                }

                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return client;
        });
    }
}

using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TcpClientExtensions
{
    // TCPクライアントでデータを送信するメソッド
    public static IObservable<int> SendAsync(this TcpClient client, string message)
    {
        return Observable.Create<int>(async observer =>
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                NetworkStream stream = client.GetStream();
                
                // 非同期でデータを送信
                await stream.WriteAsync(data, 0, data.Length);
                observer.OnNext(data.Length);
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return client;
        });
    }

    // TCPクライアントでデータを受信するメソッド
    public static IObservable<string> ReceiveAsync(this TcpClient client)
    {
        return Observable.Create<string>(async observer =>
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                // 非同期でデータを受信
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    observer.OnNext(message);
                }

                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return client;
        });
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        string server = "localhost";
        int port = 12345;

        using (TcpClient client = new TcpClient())
        {
            // サーバーに接続
            await client.ConnectAsync(server, port);

            // 定期的にデータを送信
            var sendObservable = Observable.Interval(TimeSpan.FromSeconds(2))  // 2秒ごとにメッセージを送信
                .Select(_ => client.SendAsync("Hello Server!"))               // 送信メッセージ
                .Concat();

            sendObservable.Subscribe(
                onNext: _ => Console.WriteLine("Message sent"),
                onError: ex => Console.WriteLine($"Send Error: {ex.Message}"),
                onCompleted: () => Console.WriteLine("Sending completed")
            );

            // サーバーからのメッセージを受信
            client.ReceiveAsync()
                .Subscribe(
                    onNext: message => Console.WriteLine($"Received: {message}"),
                    onError: ex => Console.WriteLine($"Receive Error: {ex.Message}"),
                    onCompleted: () => Console.WriteLine("Receiving completed")
                );

            // プログラムが終了しないようにする
            Console.ReadLine();
        }
    }
}


