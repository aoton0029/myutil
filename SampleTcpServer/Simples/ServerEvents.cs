using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpServer.Simples
{
    public enum DisconnectReason
    {
        /// <summary>
        /// Normal disconnection.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Client connection was intentionally terminated programmatically or by the server.
        /// </summary>
        Kicked = 1,
        /// <summary>
        /// Client connection timed out; server did not receive data within the timeout window.
        /// </summary>
        Timeout = 2,
        /// <summary>
        /// The connection was not disconnected.
        /// </summary>
        None = 3
    }

    public class DataSentEventArgs : EventArgs
    {
        public string IpPort { get; }
        public long BytesSent { get; }

        internal DataSentEventArgs(string ipPort, long bytesSent)
        {
            IpPort = ipPort;
            BytesSent = bytesSent;
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public string IpPort { get; }
        public ArraySegment<byte> Data { get; }

        internal DataReceivedEventArgs(string ipPort, ArraySegment<byte> data)
        {
            IpPort = ipPort;
            Data = data;
        }
    }

    public class ConnectionEventArgs : EventArgs
    {
        public string IpPort { get; }
        public DisconnectReason Reason { get; } = DisconnectReason.None;

        internal ConnectionEventArgs(string ipPort, DisconnectReason reason = DisconnectReason.None)
        {
            IpPort = ipPort;
            Reason = reason;
        }
    }

    public class ClientEvents
    {
        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<ConnectionEventArgs> Disconnected;
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DataSentEventArgs> DataSent;

        public ClientEvents()
        {

        }

        internal void HandleConnected(object sender, ConnectionEventArgs args)
        {
            Connected?.Invoke(sender, args);
        }

        internal void HandleClientDisconnected(object sender, ConnectionEventArgs args)
        {
            Disconnected?.Invoke(sender, args);
        }

        internal void HandleDataReceived(object sender, DataReceivedEventArgs args)
        {
            DataReceived?.Invoke(sender, args);
        }

        internal void HandleDataSent(object sender, DataSentEventArgs args)
        {
            DataSent?.Invoke(sender, args);
        }

    }

    public class ServerEvents
    {
        public event EventHandler<ConnectionEventArgs> ClientConnected;

        public event EventHandler<ConnectionEventArgs> ClientDisconnected;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public event EventHandler<DataSentEventArgs> DataSent;

        public ServerEvents()
        {

        }

        internal void HandleClientConnected(object sender, ConnectionEventArgs args)
        {
            ClientConnected?.Invoke(sender, args);
        }

        internal void HandleClientDisconnected(object sender, ConnectionEventArgs args)
        {
            ClientDisconnected?.Invoke(sender, args);
        }

        internal void HandleDataReceived(object sender, DataReceivedEventArgs args)
        {
            DataReceived?.Invoke(sender, args);
        }

        internal void HandleDataSent(object sender, DataSentEventArgs args)
        {
            DataSent?.Invoke(sender, args);
        }
    }
}
