TCP通信では、受信データが分割される可能性があるため、CRLF (\r\n) が確認できるまで MemoryStream にデータを蓄積し、CRLFが見つかったらそのデータを取り出すように処理する方法を紹介します。

C# TCPClientのデータ受信処理

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var client = new TcpClient("127.0.0.1", 12345);
        using var networkStream = client.GetStream();

        var buffer = new byte[1024];
        using var memoryStream = new MemoryStream();
        var crlf = Encoding.ASCII.GetBytes("\r\n");

        while (true)
        {
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0) break; // 接続が閉じられた場合

            memoryStream.Write(buffer, 0, bytesRead);

            if (ContainsCRLF(memoryStream.ToArray(), crlf))
            {
                byte[] completeData = memoryStream.ToArray();
                string receivedMessage = Encoding.ASCII.GetString(completeData).Trim();
                
                Console.WriteLine($"Received: {receivedMessage}");

                memoryStream.SetLength(0); // バッファをクリア
            }
        }
    }

    static bool ContainsCRLF(byte[] data, byte[] crlf)
    {
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (data[i] == crlf[0] && data[i + 1] == crlf[1])
                return true;
        }
        return false;
    }
}

説明

1. データ受信のバッファリング

MemoryStream にデータを蓄積し、途中で切れたデータを保持。



2. CRLFの検出

ContainsCRLF メソッドで \r\n の存在をチェック。



3. CRLFが見つかったら処理

MemoryStream からデータを取得。

Encoding.ASCII.GetString() を使って文字列化。

処理後に MemoryStream をクリア (SetLength(0)) して次のデータに備える。




この方式を使うことで、途中でデータが分割されても問題なくCRLFまでのデータを正しく処理できます。

