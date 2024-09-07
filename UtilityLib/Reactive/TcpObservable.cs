using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Reactive
{
    public static partial class ObservableExtensions
    {
        public static IObservable<string> SendAndReceive(this IObservable<string> source, TcpClientWrapper client)
        {
            return new AnonymousObservable<string>(observer =>
            {
                return source.Subscribe(async message =>
                {
                    try
                    {
                        string response = await client.SendAndReceiveAsync(message);
                        observer.OnNext(response); // 受信したメッセージを発行
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                error => observer.OnError(error),
                () => observer.OnCompleted());
            });
        }

        public static IObservable<string> Send(this IObservable<string> source, TcpClientWrapper client)
        {
            return new AnonymousObservable<string>(observer =>
            {
                return source.Subscribe(async message =>
                {
                    try
                    {
                        await client.SendAsync(message);
                        observer.OnNext(message); // 成功したメッセージを発行
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                error => observer.OnError(error),
                () => observer.OnCompleted());
            });
        }

        public static IObservable<string> Receive(this IObservable<Unit> trigger, TcpClientWrapper client)
        {
            return new AnonymousObservable<string>(observer =>
            {
                return trigger.Subscribe(async _ =>
                {
                    try
                    {
                        string receivedMessage = await client.ReceiveAsync();
                        observer.OnNext(receivedMessage); // 受信したメッセージを発行
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                error => observer.OnError(error),
                () => observer.OnCompleted());
            });
        }


    }
}
