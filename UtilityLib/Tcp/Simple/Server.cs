using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tcp.Simple
{
    public class Server
    {
        private TcpListener _listener;
        private bool _isRunning;
        private Encoding _encoding = Encoding.ASCII;

        // デリゲートを定義し、応答メソッドのシグネチャを指定
        public delegate string ResponseHandler(string receivedData);
        private ResponseHandler _responseMethod;

        public Server(string ipAddress, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            _listener.Start();
            _isRunning = true;
            Console.WriteLine("Server started...");

            // 別スレッドでリスニングを行う
            Thread listenerThread = new Thread(ListenForClients);
            listenerThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }

        // 外部から応答メソッドを設定するためのプロパティ
        public ResponseHandler ResponseMethod
        {
            set { _responseMethod = value; }
        }

        private void ListenForClients()
        {
            while (_isRunning)
            {
                // クライアント接続を待機
                TcpClient client = _listener.AcceptTcpClient();

                // クライアントごとに別スレッドで処理
                Thread clientThread = new Thread(() => HandleClientComm(client));
                clientThread.Start();
            }
        }

        private void HandleClientComm(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            try
            {
                // データを読み取る
                bytesRead = clientStream.Read(message, 0, message.Length);
                if (bytesRead == 0)
                {
                    return; // クライアントが切断
                }

                // 受信データを文字列に変換
                string receivedData = _encoding.GetString(message, 0, bytesRead);
                Console.WriteLine("Received: " + receivedData);

                // 外部で設定された応答メソッドを使用
                string responseData = _responseMethod?.Invoke(receivedData) ?? "No response method set";

                // 応答データを送信
                byte[] responseBytes = _encoding.GetBytes(responseData);
                clientStream.Write(responseBytes, 0, responseBytes.Length);
                Console.WriteLine("Sent: " + responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }

}
