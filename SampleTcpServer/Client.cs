using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpServer
{
    public class Client
    {
        public bool IsConnected { get => _isConnected; set => _isConnected = value; }

        public IPEndPoint LocalEndpoint
        {
            get
            {
                if (_client != null && _isConnected)
                {
                    return (IPEndPoint)_client.Client.LocalEndPoint;
                }

                return null;
            }
        }

        public ClientSetting Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                if (value == null) _settings = new ClientSetting();
                else _settings = value;
            }
        }

        public ClientEvents Events
        {
            get
            {
                return _events;
            }
            set
            {
                if (value == null) _events = new ClientEvents();
                else _events = value;
            }
        }

        public Statistic Statistics
        {
            get
            {
                return _statistics;
            }
        }

        public KeepaliveSetting Keepalive
        {
            get
            {
                return _keepalive;
            }
            set
            {
                if (value == null) _keepalive = new KeepaliveSetting();
                else _keepalive = value;
            }
        }

        public Action<string> Logger = null;

        public string ServerIpPort
        {
            get
            {
                return $"{_serverIp}:{_serverPort}";
            }
        }

        private readonly string _header = "[SimpleTcp.Client] ";
        private ClientSetting _settings = new ClientSetting();
        private ClientEvents _events = new ClientEvents();
        private KeepaliveSetting _keepalive = new KeepaliveSetting();
        private Statistic _statistics = new Statistic();

        private string _serverIp = null;
        private int _serverPort = 0;
        private readonly IPAddress _ipAddress = null;
        private TcpClient _client = null;
        private NetworkStream _networkStream = null;

        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private bool _isConnected = false;

        private Task _dataReceiver = null;
        private Task _idleServerMonitor = null;
        private Task _connectionMonitor = null;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private CancellationToken _token;

        private DateTime _lastActivity = DateTime.Now;
        private bool _isTimeout = false;

        /// <summary>
        /// Instantiates the TCP client without SSL. 
        /// Set the Connected, Disconnected, and DataReceived callbacks. Once set, use Connect() to connect to the server.
        /// </summary>
        /// <param name="ipPort">The IP:port of the server.</param> 
        public Client(string ipPort)
        {
            if (string.IsNullOrEmpty(ipPort)) throw new ArgumentNullException(nameof(ipPort));

            Common.ParseIpPort(ipPort, out _serverIp, out _serverPort);
            if (_serverPort < 0) throw new ArgumentException("Port must be zero or greater.");
            if (string.IsNullOrEmpty(_serverIp)) throw new ArgumentNullException("Server IP or hostname must not be null.");

            if (!IPAddress.TryParse(_serverIp, out _ipAddress))
            {
                _ipAddress = Dns.GetHostEntry(_serverIp).AddressList[0];
                _serverIp = _ipAddress.ToString();
            }
        }

        /// <summary>
        /// Instantiates the TCP client without SSL. 
        /// Set the Connected, Disconnected, and DataReceived callbacks. Once set, use Connect() to connect to the server.
        /// </summary>
        /// <param name="serverIpOrHostname">The server IP address or hostname.</param>
        /// <param name="port">The TCP port on which to connect.</param>
        public Client(string serverIpOrHostname, int port)
        {
            if (string.IsNullOrEmpty(serverIpOrHostname)) throw new ArgumentNullException(nameof(serverIpOrHostname));
            if (port < 0) throw new ArgumentException("Port must be zero or greater.");

            _serverIp = serverIpOrHostname;
            _serverPort = port;

            if (!IPAddress.TryParse(_serverIp, out _ipAddress))
            {
                _ipAddress = Dns.GetHostEntry(serverIpOrHostname).AddressList[0];
                _serverIp = _ipAddress.ToString();
            }
        }



        public Client(IPAddress serverIpAddress, int port) : this(new IPEndPoint(serverIpAddress, port))
        {
        }

        public Client(IPEndPoint serverIpEndPoint)
        {
            if (serverIpEndPoint == null) throw new ArgumentNullException(nameof(serverIpEndPoint));
            else if (serverIpEndPoint.Port < 0) throw new ArgumentException("Port must be zero or greater.");
            else
            {
                _ipAddress = serverIpEndPoint.Address;
                _serverIp = serverIpEndPoint.Address.ToString();
                _serverPort = serverIpEndPoint.Port;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Connect()
        {
            if (IsConnected)
            {
                Logger?.Invoke($"{_header}already connected");
                return;
            }
            else
            {
                Logger?.Invoke($"{_header}initializing client");
                InitializeClient();
                Logger?.Invoke($"{_header}connecting to {ServerIpPort}");
            }

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _token.Register(() =>
            {
                if (_networkStream == null) return;
                _networkStream.Close();
            });

            IAsyncResult ar = _client.BeginConnect(_serverIp, _serverPort, null, null);
            WaitHandle wh = ar.AsyncWaitHandle;

            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(_settings.ConnectTimeoutMs), false))
                {
                    _client.Close();
                    throw new TimeoutException($"Timeout connecting to {ServerIpPort}");
                }

                _client.EndConnect(ar);
                _networkStream = _client.GetStream();
                _networkStream.ReadTimeout = _settings.ReadTimeoutMs;

                if (_keepalive.EnableTcpKeepAlives) EnableKeepalives();
            }
            catch (Exception)
            {
                throw;
            }

            _isConnected = true;
            _lastActivity = DateTime.Now;
            _isTimeout = false;
            _events.HandleConnected(this, new ConnectionEventArgs(ServerIpPort));
            _dataReceiver = Task.Run(() => DataReceiver(_token), _token);
            _idleServerMonitor = Task.Run(IdleServerMonitor, _token);
            _connectionMonitor = Task.Run(ConnectedMonitor, _token);
        }

        public void InitializeClient()
        {
            _client = _settings.LocalEndpoint == null ? new TcpClient() : new TcpClient(_settings.LocalEndpoint);
            _client.NoDelay = _settings.NoDelay;
        }

        public void ConnectWithRetries(int? timeoutMs = null)
        {
            if (timeoutMs != null && timeoutMs < 1) throw new ArgumentException("Timeout milliseconds must be greater than zero.");
            if (timeoutMs != null) _settings.ConnectTimeoutMs = timeoutMs.Value;

            if (IsConnected)
            {
                Logger?.Invoke($"{_header}already connected");
                return;
            }
            else
            {
                Logger?.Invoke($"{_header}initializing client");

                InitializeClient();

                Logger?.Invoke($"{_header}connecting to {ServerIpPort}");
            }

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _token.Register(() =>
            {
                if (_networkStream == null) return;
                _networkStream.Close();
            });


            using (CancellationTokenSource connectTokenSource = new CancellationTokenSource())
            {
                CancellationToken connectToken = connectTokenSource.Token;

                Task cancelTask = Task.Delay(_settings.ConnectTimeoutMs, _token);
                Task connectTask = Task.Run(() =>
                {
                    int retryCount = 0;

                    while (true)
                    {
                        try
                        {
                            string msg = $"{_header}attempting connection to {_serverIp}:{_serverPort}";
                            if (retryCount > 0) msg += $" ({retryCount} retries)";
                            Logger?.Invoke(msg);

                            _client.Dispose();
                            _client = _settings.LocalEndpoint == null ? new TcpClient() : new TcpClient(_settings.LocalEndpoint);
                            _client.NoDelay = _settings.NoDelay;
                            _client.ConnectAsync(_serverIp, _serverPort).Wait(1000, connectToken);

                            if (_client.Connected)
                            {
                                Logger?.Invoke($"{_header}connected to {_serverIp}:{_serverPort}");
                                break;
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception e)
                        {
                            Logger?.Invoke($"{_header}failed connecting to {_serverIp}:{_serverPort}: {e.Message}");
                        }
                        finally
                        {
                            retryCount++;
                        }
                    }
                }, connectToken);

                Task.WhenAny(cancelTask, connectTask).Wait();

                if (cancelTask.IsCompleted)
                {
                    connectTokenSource.Cancel();
                    _client.Close();
                    throw new TimeoutException($"Timeout connecting to {ServerIpPort}");
                }

                try
                {
                    _networkStream = _client.GetStream();
                    _networkStream.ReadTimeout = _settings.ReadTimeoutMs;

                    if (_keepalive.EnableTcpKeepAlives) EnableKeepalives();
                }
                catch (Exception)
                {
                    throw;
                }

            }

            _isConnected = true;
            _lastActivity = DateTime.Now;
            _isTimeout = false;
            _events.HandleConnected(this, new ConnectionEventArgs(ServerIpPort));
            _dataReceiver = Task.Run(() => DataReceiver(_token), _token);
            _idleServerMonitor = Task.Run(IdleServerMonitor, _token);
            _connectionMonitor = Task.Run(ConnectedMonitor, _token);
        }

        public void Disconnect()
        {
            if (!IsConnected)
            {
                Logger?.Invoke($"{_header}already disconnected");
                return;
            }

            Logger?.Invoke($"{_header}disconnecting from {ServerIpPort}");

            _tokenSource.Cancel();
            WaitCompletion();
            _client.Close();
            _isConnected = false;
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected)
            {
                Logger?.Invoke($"{_header}already disconnected");
                return;
            }

            Logger?.Invoke($"{_header}disconnecting from {ServerIpPort}");

            _tokenSource.Cancel();
            await WaitCompletionAsync();
            _client.Close();
            _isConnected = false;
        }

        public void Send(string data)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            if (!_isConnected) throw new IOException("Not connected to the server; use Connect() first.");

            byte[] bytes = Encoding.UTF8.GetBytes(data);
            this.Send(bytes);
        }

        public void Send(byte[] data)
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
            if (!_isConnected) throw new IOException("Not connected to the server; use Connect() first.");

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);
                SendInternal(data.Length, ms);
            }
        }

        public void Send(long contentLength, Stream stream)
        {
            if (contentLength < 1) return;
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new InvalidOperationException("Cannot read from supplied stream.");
            if (!_isConnected) throw new IOException("Not connected to the server; use Connect() first.");

            SendInternal(contentLength, stream);
        }

        public async Task SendAsync(string data, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            if (!_isConnected) throw new IOException("Not connected to the server; use Connect() first.");
            if (token == default(CancellationToken)) token = _token;

            byte[] bytes = Encoding.UTF8.GetBytes(data);

            using (MemoryStream ms = new MemoryStream())
            {
                await ms.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
                ms.Seek(0, SeekOrigin.Begin);
                await SendInternalAsync(bytes.Length, ms, token).ConfigureAwait(false);
            }
        }

        public async Task SendAsync(byte[] data, CancellationToken token = default)
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
            if (!_isConnected) throw new IOException("Not connected to the server; use Connect() first.");
            if (token == default(CancellationToken)) token = _token;

            using (MemoryStream ms = new MemoryStream())
            {
                await ms.WriteAsync(data, 0, data.Length, token).ConfigureAwait(false);
                ms.Seek(0, SeekOrigin.Begin);
                await SendInternalAsync(data.Length, ms, token).ConfigureAwait(false);
            }
        }

        public async Task SendAsync(long contentLength, Stream stream, CancellationToken token = default)
        {
            if (contentLength < 1) return;
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new InvalidOperationException("Cannot read from supplied stream.");
            if (!_isConnected) throw new IOException("Not connected to the server; use Connect() first.");
            if (token == default(CancellationToken)) token = _token;

            await SendInternalAsync(contentLength, stream, token).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isConnected = false;

                if (_tokenSource != null)
                {
                    if (!_tokenSource.IsCancellationRequested)
                    {
                        _tokenSource.Cancel();
                        _tokenSource.Dispose();
                    }
                }

                if (_networkStream != null)
                {
                    _networkStream.Close();
                    _networkStream.Dispose();
                }

                if (_client != null)
                {
                    _client.Close();
                    _client.Dispose();
                }

                Logger?.Invoke($"{_header}dispose complete");
            }
        }

        private async Task DataReceiver(CancellationToken token)
        {
            Stream outerStream = null;
            outerStream = _networkStream;
            while (!token.IsCancellationRequested && _client != null && _client.Connected)
            {
                try
                {
                    await DataReadAsync(token).ContinueWith(async task =>
                    {
                        if (task.IsCanceled) return default;
                        var data = task.Result;

                        if (data != null)
                        {
                            _lastActivity = DateTime.Now;

                            Action action = () => _events.HandleDataReceived(this, new DataReceivedEventArgs(ServerIpPort, data));
                            if (_settings.UseAsyncDataReceivedEvents)
                            {
                                _ = Task.Run(action, token);
                            }
                            else
                            {
                                action.Invoke();
                            }

                            _statistics.ReceivedBytes += data.Count;

                            return data;
                        }
                        else
                        {
                            await Task.Delay(100).ConfigureAwait(false);
                            return default;
                        }

                    }, token).ContinueWith(task => { }).ConfigureAwait(false);
                }
                catch (AggregateException)
                {
                    Logger?.Invoke($"{_header}data receiver canceled, disconnected");
                    break;
                }
                catch (IOException)
                {
                    Logger?.Invoke($"{_header}data receiver canceled, disconnected");
                    break;
                }
                catch (SocketException)
                {
                    Logger?.Invoke($"{_header}data receiver canceled, disconnected");
                    break;
                }
                catch (TaskCanceledException)
                {
                    Logger?.Invoke($"{_header}data receiver task canceled, disconnected");
                    break;
                }
                catch (OperationCanceledException)
                {
                    Logger?.Invoke($"{_header}data receiver operation canceled, disconnected");
                    break;
                }
                catch (ObjectDisposedException)
                {
                    Logger?.Invoke($"{_header}data receiver canceled due to disposal, disconnected");
                    break;
                }
                catch (Exception e)
                {
                    Logger?.Invoke($"{_header}data receiver exception:{Environment.NewLine}{e}{Environment.NewLine}");
                    break;
                }
            }

            Logger?.Invoke($"{_header}disconnection detected");

            _isConnected = false;

            if (!_isTimeout) _events.HandleClientDisconnected(this, new ConnectionEventArgs(ServerIpPort, DisconnectReason.Normal));
            else _events.HandleClientDisconnected(this, new ConnectionEventArgs(ServerIpPort, DisconnectReason.Timeout));

            Dispose();
        }

        private async Task<ArraySegment<byte>> DataReadAsync(CancellationToken token)
        {
            byte[] buffer = new byte[_settings.StreamBufferSize];
            int read = 0;

            try
            {
                read = await _networkStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);

                if (read > 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(buffer, 0, read);
                        return new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Length);
                    }
                }
                else
                {
                    IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                    TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections()
                        .Where(x => x.LocalEndPoint.Equals(this._client.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(this._client.Client.RemoteEndPoint)).ToArray();

                    var isOk = false;

                    if (tcpConnections != null && tcpConnections.Length > 0)
                    {
                        TcpState stateOfConnection = tcpConnections.First().State;
                        if (stateOfConnection == TcpState.Established)
                        {
                            isOk = true;
                        }
                    }

                    if (!isOk)
                    {
                        await this.DisconnectAsync();
                    }

                    throw new SocketException();
                }
            }
            catch (IOException)
            {
                return default;
            }
        }

        private void SendInternal(long contentLength, Stream stream)
        {
            long bytesRemaining = contentLength;
            int bytesRead = 0;
            byte[] buffer = new byte[_settings.StreamBufferSize];

            try
            {
                _sendLock.Wait();

                while (bytesRemaining > 0)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        _networkStream.Write(buffer, 0, bytesRead);

                        bytesRemaining -= bytesRead;
                        _statistics.SentBytes += bytesRead;
                    }
                }

                _networkStream.Flush();
                _events.HandleDataSent(this, new DataSentEventArgs(ServerIpPort, contentLength));
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task SendInternalAsync(long contentLength, Stream stream, CancellationToken token)
        {
            try
            {
                long bytesRemaining = contentLength;
                int bytesRead = 0;
                byte[] buffer = new byte[_settings.StreamBufferSize];

                await _sendLock.WaitAsync(token).ConfigureAwait(false);

                while (bytesRemaining > 0)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                    if (bytesRead > 0)
                    {
                        await _networkStream.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                        bytesRemaining -= bytesRead;
                        _statistics.SentBytes += bytesRead;
                    }
                }

                await _networkStream.FlushAsync(token).ConfigureAwait(false); 
                _events.HandleDataSent(this, new DataSentEventArgs(ServerIpPort, contentLength));
            }
            catch (TaskCanceledException)
            {

            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                _sendLock.Release();
            }
        }

        private void WaitCompletion()
        {
            try
            {
                _dataReceiver.Wait();
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                Logger?.Invoke("Awaiting a canceled task");
            }
        }

        private async Task WaitCompletionAsync()
        {
            try
            {
                await _dataReceiver;
            }
            catch (TaskCanceledException)
            {
                Logger?.Invoke("Awaiting a canceled task");
            }
        }

        private void EnableKeepalives()
        {
            // issues with definitions: https://github.com/dotnet/sdk/issues/14540

            try
            {
                byte[] keepAlive = new byte[12];

                // Turn keepalive on
                Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, keepAlive, 0, 4);

                // Set TCP keepalive time
                Buffer.BlockCopy(BitConverter.GetBytes((uint)_keepalive.TcpKeepAliveTimeMilliseconds), 0, keepAlive, 4, 4);

                // Set TCP keepalive interval
                Buffer.BlockCopy(BitConverter.GetBytes((uint)_keepalive.TcpKeepAliveIntervalMilliseconds), 0, keepAlive, 8, 4);

                // Set keepalive settings on the underlying Socket
                _client.Client.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);

            }
            catch (Exception)
            {
                Logger?.Invoke($"{_header}keepalives not supported on this platform, disabled");
                _keepalive.EnableTcpKeepAlives = false;
            }
        }

        private async Task IdleServerMonitor()
        {
            while (!_token.IsCancellationRequested)
            {
                await Task.Delay(_settings.IdleServerEvaluationIntervalMs, _token).ConfigureAwait(false);

                if (_settings.IdleServerTimeoutMs == 0) continue;

                DateTime timeoutTime = _lastActivity.AddMilliseconds(_settings.IdleServerTimeoutMs);

                if (DateTime.Now > timeoutTime)
                {
                    Logger?.Invoke($"{_header}disconnecting from {ServerIpPort} due to timeout");
                    _isConnected = false;
                    _isTimeout = true;
                    _tokenSource.Cancel(); // DataReceiver will fire events including dispose
                }
            }
        }

        private async Task ConnectedMonitor()
        {
            while (!_token.IsCancellationRequested)
            {
                await Task.Delay(_settings.ConnectionLostEvaluationIntervalMs, _token).ConfigureAwait(false);

                if (!_isConnected)
                    continue; //Just monitor connected clients

                if (!PollSocket())
                {
                    Logger?.Invoke($"{_header}disconnecting from {ServerIpPort} due to connection lost");
                    _isConnected = false;
                    _tokenSource.Cancel(); // DataReceiver will fire events including dispose
                }
            }
        }

        private bool PollSocket()
        {
            try
            {
                if (_client.Client == null || !_client.Client.Connected)
                    return false;

                /* pear to the documentation on Poll:
                 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                 * -or- true if data is available for reading; 
                 * -or- true if the connection has been closed, reset, or terminated; 
                 * otherwise, returns false
                 */
                if (!_client.Client.Poll(0, SelectMode.SelectRead))
                    return true;

                var buff = new byte[1];
                var clientSentData = _client.Client.Receive(buff, SocketFlags.Peek) != 0;
                return clientSentData; //False here though Poll() succeeded means we had a disconnect!
            }
            catch (SocketException ex)
            {
                Logger?.Invoke($"{_header}poll socket from {ServerIpPort} failed with ex = {ex}");
                return ex.SocketErrorCode == SocketError.TimedOut;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
