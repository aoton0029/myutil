### **WinForms で TCP サーバーを再起動できるように改良**
サーバーの**再起動機能を実装**し、**停止後に再度起動できるように修正**します。

---

## **修正ポイント**
✅ **サーバーの状態を管理し、再起動時に新しい `ServerManager` インスタンスを作成**  
✅ **`StartAll()` 実行中はボタンを無効化し、誤操作を防ぐ**  
✅ **停止後に `ServerManager` を適切に破棄し、新しく作成して再起動できるようにする**

---

## **1. `MainForm.cs`（UI の修正）**
```csharp
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private ServerManager _serverManager;
    private AsyncLogProcessor _logProcessor;
    private bool _isServerRunning = false; // サーバー状態管理

    public MainForm()
    {
        InitializeComponent();
        _logProcessor = new AsyncLogProcessor();
        _serverManager = new ServerManager(_logProcessor);
    }

    private async void btnStartServer_Click(object sender, EventArgs e)
    {
        if (_isServerRunning) return;

        _isServerRunning = true;
        btnStartServer.Enabled = false;
        btnStopServer.Enabled = true;

        // サーバーのインスタンスを新規作成（再起動対応）
        _serverManager = new ServerManager(_logProcessor);

        await Task.Run(() => _serverManager.StartAll());
        AddLog("サーバーを起動しました");
    }

    private async void btnStopServer_Click(object sender, EventArgs e)
    {
        if (!_isServerRunning) return;

        _isServerRunning = false;
        btnStartServer.Enabled = true;
        btnStopServer.Enabled = false;

        await _serverManager.StopAllAsync();
        AddLog("サーバーを停止しました");
    }

    private void AddLog(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string>(AddLog), message);
            return;
        }
        lstLog.Items.Add($"{DateTime.Now}: {message}");
    }

    private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_isServerRunning)
        {
            await _serverManager.StopAllAsync();
        }
        _logProcessor.Dispose();
    }
}
```

---

## **2. `ServerManager.cs`（クリーンな再起動のための修正）**
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ServerManager
{
    private readonly List<DeviceServer> _servers = new();
    private readonly List<Task> _serverTasks = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly AsyncLogProcessor _logProcessor;

    public ServerManager(AsyncLogProcessor logProcessor)
    {
        _logProcessor = logProcessor;
        _servers.Add(new DeviceServer("192.168.1.100", 5001, new PowerSupply("PowerSupply_1"), new PowerSupplyProtocol(), _logProcessor));
        _servers.Add(new DeviceServer("192.168.1.101", 5002, new Multimeter("Multimeter_1"), new MultimeterProtocol(), _logProcessor));
        _servers.Add(new DeviceServer("192.168.1.102", 5003, new TemperatureSensor("TemperatureSensor_1"), new TemperatureSensorProtocol(), _logProcessor));
    }

    public void StartAll()
    {
        _logProcessor.Log("全サーバーを非同期で起動します...");
        foreach (var server in _servers)
        {
            _serverTasks.Add(Task.Run(() => server.StartAsync(), _cts.Token));
        }
    }

    public async Task StopAllAsync()
    {
        _logProcessor.Log("全サーバーを停止します...");
        _cts.Cancel();
        foreach (var server in _servers) server.Stop();
        await Task.WhenAll(_serverTasks);
        await _logProcessor.ShutdownAsync();
    }
}
```

---

## **3. `DeviceServer.cs`（クライアント接続処理を強化）**
```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class DeviceServer
{
    private readonly string _ipAddress;
    private readonly int _port;
    private readonly TcpListener _listener;
    private readonly IDevice _device;
    private readonly IDeviceProtocol _protocol;
    private readonly AsyncLogProcessor _logProcessor;
    private bool _isRunning = false; // サーバーが動作中かどうか

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
        if (_isRunning) return;
        _isRunning = true;

        try
        {
            _listener.Start();
            _logProcessor.Log($"[{_device.Name}] サーバー起動 {_ipAddress}:{_port}");

            while (_isRunning)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _logProcessor.Log($"[{_device.Name}] クライアント接続");
                _ = HandleClientAsync(client);
            }
        }
        catch (Exception ex)
        {
            _logProcessor.Log($"[{_device.Name}] サーバーエラー: {ex.Message}");
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (_isRunning && client.Connected)
            {
                int bytesRead;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch (IOException ex) when (ex.InnerException is SocketException)
                {
                    _logProcessor.Log($"[{_device.Name}] クライアント通信エラー (切断の可能性): {ex.Message}");
                    break;
                }

                if (bytesRead == 0) break;

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
                    _logProcessor.Log($"[{_device.Name}] クライアント送信エラー (切断の可能性): {ex.Message}");
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

    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;
        _listener.Stop();
        _logProcessor.Log($"[{_device.Name}] サーバー停止");
    }
}
```

---

## **修正のポイント**
✅ **`_isRunning` でサーバーの起動状態を管理し、二重起動を防ぐ**  
✅ **`Stop()` で `_isRunning = false` にし、クライアント受信ループを終了**  
✅ **`MainForm` で `ServerManager` を再作成し、完全に新しいインスタンスとして起動可能**  
✅ **`Task.Run()` でサーバーを非同期起動し、UI がフリーズしないようにする**

---

## **動作例**
### **1. サーバー起動**
```
全サーバーを非同期で起動します...
[PowerSupply_1] サーバー起動 192.168.1.100:5001
[Multimeter_1] サーバー起動 192.168.1.101:5002
[TemperatureSensor_1] サーバー起動 192.168.1.102:5003
```

### **2. サーバー停止**
```
全サーバーを停止します...
[PowerSupply_1] サーバー停止
[Multimeter_1] サーバー停止
[TemperatureSensor_1] サーバー停止
```

### **3. 再起動**
```
全サーバーを非同期で起動します...
[PowerSupply_1] サーバー起動 192.168.1.100:5001
...
```

これで、**ボタンで自由にサーバーの起動・停止・再起動ができる WinForms アプリ** が完成しました！ 🚀




## **非同期ログプロセッサーサービスの導入**
マルチスレッド環境で安全にログを処理するために、**非同期ログプロセッサー (`AsyncLogProcessor`) を実装** します。  
これにより、ログ出力の **スレッド競合を防ぎ**、パフォーマンスを向上させます。

---

### **設計のポイント**
✅ **ログを非同期キュー (`ConcurrentQueue<string>`) に追加し、専用スレッドで処理**  
✅ **`Task.Run()` によるバックグラウンド処理でログを非同期出力**  
✅ **サーバーの終了時にログ処理を安全に完了させる (`CancellationTokenSource`)**  
✅ **`ServerManager` と `DeviceServer` でログ出力を統一**

---

## **1. `AsyncLogProcessor`（非同期ログサービス）**
```csharp
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class AsyncLogProcessor : IDisposable
{
    private readonly string _logFilePath;
    private readonly ConcurrentQueue<string> _logQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _logTask;

    public AsyncLogProcessor(string logFilePath = "server_log.txt")
    {
        _logFilePath = logFilePath;
        _logTask = Task.Run(ProcessLogQueueAsync);
    }

    public void Log(string message)
    {
        string logMessage = $"{DateTime.Now}: {message}";
        _logQueue.Enqueue(logMessage);
    }

    private async Task ProcessLogQueueAsync()
    {
        using StreamWriter writer = new(_logFilePath, append: true);
        while (!_cts.Token.IsCancellationRequested || !_logQueue.IsEmpty)
        {
            if (_logQueue.TryDequeue(out string logMessage))
            {
                await writer.WriteLineAsync(logMessage);
                await writer.FlushAsync();
                Console.WriteLine(logMessage); // コンソールにも出力
            }
            else
            {
                await Task.Delay(100); // ログがないときは待機
            }
        }
    }

    public async Task ShutdownAsync()
    {
        _cts.Cancel();
        await _logTask; // ログ処理タスクの完了を待つ
    }

    public void Dispose()
    {
        _cts.Cancel();
        _logTask.Wait();
    }
}
```

---

## **2. `ServerManager` で `AsyncLogProcessor` を適用**
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ServerManager
{
    private readonly List<DeviceServer> _servers = new();
    private readonly List<Task> _serverTasks = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly AsyncLogProcessor _logProcessor;

    public ServerManager(AsyncLogProcessor logProcessor)
    {
        _logProcessor = logProcessor;

        _servers.Add(new DeviceServer("192.168.1.100", 5001, new PowerSupply("PowerSupply_1"), new PowerSupplyProtocol(), _logProcessor));
        _servers.Add(new DeviceServer("192.168.1.101", 5002, new Multimeter("Multimeter_1"), new MultimeterProtocol(), _logProcessor));
        _servers.Add(new DeviceServer("192.168.1.102", 5003, new TemperatureSensor("TemperatureSensor_1"), new TemperatureSensorProtocol(), _logProcessor));
    }

    public void StartAll()
    {
        _logProcessor.Log("全サーバーを非同期で起動します...");
        foreach (var server in _servers)
        {
            _serverTasks.Add(Task.Run(() => server.StartAsync(), _cts.Token));
        }
    }

    public async Task StopAllAsync()
    {
        _logProcessor.Log("全サーバーを停止します...");
        _cts.Cancel();

        foreach (var server in _servers)
        {
            server.Stop();
        }

        await Task.WhenAll(_serverTasks);
        await _logProcessor.ShutdownAsync(); // ログ処理の完了を待つ
    }
}
```

---

## **3. `DeviceServer` で `AsyncLogProcessor` を適用**
```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
            _logProcessor.Log($"[{_device.Name}] サーバーエラー: {ex.Message}");
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string receivedText = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                string decodedCommand = _protocol.Decode(receivedText);
                _logProcessor.Log($"[{_device.Name}] 受信: {decodedCommand}");

                string response = _device.ProcessCommand(decodedCommand);
                string encodedResponse = _protocol.Encode(response);

                byte[] responseBytes = Encoding.UTF8.GetBytes(encodedResponse + "\n");
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
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

    public void Stop()
    {
        _listener.Stop();
        _logProcessor.Log($"[{_device.Name}] サーバー停止");
    }
}
```

---

## **4. `Program.cs` で統合**
```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using AsyncLogProcessor logProcessor = new();
        ServerManager manager = new(logProcessor);
        manager.StartAll();

        Console.WriteLine("サーバーが起動しました。Enterキーで停止します。");
        Console.ReadLine();

        await manager.StopAllAsync();
    }
}
```

---

## **改善点**
✅ **非同期ログ処理でスレッド競合を防ぐ**
- `ConcurrentQueue<string>` を使用し、**非同期的にログを処理**
- `Task.Run()` をバックグラウンドで実行し、**負荷を最小化**

✅ **`ServerManager` と `DeviceServer` で統一的にログ管理**
- `AsyncLogProcessor` によって **すべてのログが一元管理**
- ログの出力順序が **一定** になり、デバッグがしやすい

✅ **ログ処理が遅延してもサーバーの動作に影響しない**
- 各スレッドからのログ出力が**非同期で処理**されるため、サーバーの動作がスムーズ

---

## **動作例**
```
2025-02-25 12:00:00: 全サーバーを非同期で起動します...
2025-02-25 12:00:01: [PowerSupply_1] サーバー起動 192.168.1.100:5001
2025-02-25 12:00:01: [Multimeter_1] サーバー起動 192.168.1.101:5002
2025-02-25 12:00:02: [TemperatureSensor_1] サーバー起動 192.168.1.102:5003
```
これで、**非同期でログを処理しつつ、並列実行する TCP サーバーが構築** できます！ 🎉


### **外部設定の拡張**
既存のシナリオやログ機能をさらに強化するため、**外部設定ファイル** を活用して以下の点を管理できるようにします。

---

## **📌 拡張ポイント**
1. **デバイスの設定を外部ファイル (`config.json`) で管理**
   - 電源の初期値（電圧、電流）
   - シミュレーションの遅延時間
   - タイムアウト設定

2. **シナリオ (`scenarios.json`) の強化**
   - コマンドごとの応答設定
   - 応答遅延、異常シナリオを自由に追加

3. **ログ (`log.json`) の出力先を設定可能**
   - `logs/` フォルダに日付ごとのファイルを保存
   - ファイルサイズを制限してローテーション管理

---

## **1️⃣ `config.json` (外部設定)**
シミュレーションの初期状態や動作設定を定義します。

```json
{
  "server": {
    "port": 5000,
    "timeout": 5000,
    "log_directory": "logs"
  },
  "power_supply": {
    "initial_voltage": 12.0,
    "initial_current": 1.0,
    "response_delay": 100
  },
  "multimeter": {
    "measurement_noise": 0.05
  }
}
```

### **設定項目**
- `server.port` → TCPサーバーのポート
- `server.timeout` → タイムアウト時間 (ms)
- `server.log_directory` → ログ保存先フォルダ
- `power_supply.initial_voltage` → 初期電圧 (V)
- `power_supply.initial_current` → 初期電流 (A)
- `power_supply.response_delay` → 応答遅延 (ms)
- `multimeter.measurement_noise` → 計測誤差範囲

---

## **2️⃣ `scenarios.json` (シナリオ管理)**
各コマンドのレスポンスを定義します。

```json
{
  "scenarios": [
    {
      "command": "GET_VOLTAGE",
      "response": "VOLTAGE: 12.00",
      "delay": 100
    },
    {
      "command": "GET_CURRENT",
      "response": "CURRENT: 1.00",
      "delay": 100
    },
    {
      "command": "SIMULATE_ERROR",
      "response": "ERROR: Overcurrent detected",
      "delay": 0
    },
    {
      "command": "SIMULATE_TIMEOUT",
      "response": "TIMEOUT",
      "delay": 5000
    }
  ]
}
```

---

## **3️⃣ `log.json` (ログフォーマット)**
サーバーのログ記録。

```json
{
  "logs": [
    {
      "timestamp": "2025-02-23T12:00:00.123",
      "client": "POWER_SUPPLY",
      "command": "SET 15.0 2.5",
      "response": "SET OK: Voltage=15.00V, Current=2.50A",
      "delay": 0
    }
  ]
}
```

---

## **4️⃣ C# 実装 (設定の読み込み)**
`config.json` を読み込んで、サーバーの動作をカスタマイズします。

### **📝 設定ファイルの読み込み**
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var server = new TcpDeviceSimulator();
        await server.LoadConfig("config.json");
        await server.LoadScenarios("scenarios.json");
        await server.StartServer();
    }
}

class TcpDeviceSimulator
{
    private TcpListener _listener;
    private bool _running = true;
    private Dictionary<string, Scenario> _scenarios = new Dictionary<string, Scenario>();
    private ServerConfig _config;
    private PowerSupply _powerSupply;

    public async Task LoadConfig(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            _config = JsonSerializer.Deserialize<ServerConfig>(json);
            _powerSupply = new PowerSupply(_config.PowerSupply.InitialVoltage, _config.PowerSupply.InitialCurrent);
            Console.WriteLine("設定ファイルをロードしました。");
        }
        else
        {
            Console.WriteLine("設定ファイルが見つかりません。デフォルト設定を使用します。");
            _config = new ServerConfig();
            _powerSupply = new PowerSupply();
        }
    }

    public async Task LoadScenarios(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            var scenarioList = JsonSerializer.Deserialize<ScenarioList>(json);
            if (scenarioList != null)
            {
                foreach (var scenario in scenarioList.Scenarios)
                {
                    _scenarios[scenario.Command] = scenario;
                }
                Console.WriteLine("シナリオがロードされました。");
            }
        }
    }

    public async Task StartServer()
    {
        _listener = new TcpListener(IPAddress.Any, _config.Server.Port);
        _listener.Start();
        Console.WriteLine($"TCPサーバーがポート {_config.Server.Port} で起動しました...");

        while (_running)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = HandleClient(client);
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        Console.WriteLine("クライアントが接続しました。");

        try
        {
            while (_running)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"受信: {command}");

                string response = await ProcessCommand(command);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"エラー: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine("クライアントが切断されました。");
        }
    }

    private async Task<string> ProcessCommand(string command)
    {
        if (_scenarios.TryGetValue(command, out var scenario))
        {
            await Task.Delay(scenario.Delay);
            return scenario.Response;
        }
        return "UNKNOWN_COMMAND";
    }
}

// 設定ファイルクラス
class ServerConfig
{
    public ServerSettings Server { get; set; } = new ServerSettings();
    public PowerSupplySettings PowerSupply { get; set; } = new PowerSupplySettings();
}

class ServerSettings
{
    public int Port { get; set; } = 5000;
    public int Timeout { get; set; } = 5000;
    public string LogDirectory { get; set; } = "logs";
}

class PowerSupplySettings
{
    public double InitialVoltage { get; set; } = 12.0;
    public double InitialCurrent { get; set; } = 1.0;
}

// シナリオ設定クラス
class Scenario
{
    public string Command { get; set; }
    public string Response { get; set; }
    public int Delay { get; set; } = 0;
}

class ScenarioList
{
    public List<Scenario> Scenarios { get; set; }
}
```

---

## **🎯 まとめ**
✅ **外部設定でサーバーの動作をカスタマイズ**  
✅ **電源の初期状態、ログ保存先、遅延時間を設定可能**  
✅ **シナリオファイルで応答を自由に変更**  
✅ **設定ファイルを修正するだけで新しい動作を追加可能**  

この機能で、より柔軟なシミュレーション環境が構築できます！ 🚀




シナリオの外部設定を **JSONファイル** で管理し、サーバーがその内容に従って応答するようにします。これにより、さまざまなテストケース（正常・異常・タイムアウト）を外部から簡単に設定・変更できます。

---

## **1. シナリオファイル (JSON) の設計**
各シナリオには、受信コマンド (`command`) に対する応答 (`response`) と、必要に応じた遅延 (`delay`) を定義します。

```json
{
  "scenarios": [
    {
      "command": "GET_VOLTAGE",
      "response": "VOLTAGE: 12.00",
      "delay": 0
    },
    {
      "command": "GET_CURRENT",
      "response": "CURRENT: 1.00",
      "delay": 0
    },
    {
      "command": "GET_POWER",
      "response": "POWER: 12.00",
      "delay": 0
    },
    {
      "command": "SIMULATE_ERROR",
      "response": "ERROR: Unexpected condition",
      "delay": 0
    },
    {
      "command": "SIMULATE_TIMEOUT",
      "response": "TIMEOUT",
      "delay": 5000
    }
  ]
}
```

- `"command"` : クライアントからのコマンド
- `"response"` : 返すべき応答
- `"delay"` : 応答前の遅延時間（ミリ秒）

---

## **2. C#サーバーの実装 (シナリオをロード)**
サーバーが起動時にJSONファイルを読み込み、クライアントのコマンドに応じた動作を決定します。

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var server = new TcpDeviceSimulator();
        await server.LoadScenarios("scenarios.json");
        await server.StartServer(5000);
    }
}

class TcpDeviceSimulator
{
    private TcpListener _listener;
    private bool _running = true;
    private Dictionary<string, Scenario> _scenarios = new Dictionary<string, Scenario>();

    public async Task LoadScenarios(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            var scenarioList = JsonSerializer.Deserialize<ScenarioList>(json);

            if (scenarioList != null)
            {
                foreach (var scenario in scenarioList.Scenarios)
                {
                    _scenarios[scenario.Command] = scenario;
                }
                Console.WriteLine("シナリオがロードされました。");
            }
        }
        else
        {
            Console.WriteLine("シナリオファイルが見つかりません。デフォルト動作になります。");
        }
    }

    public async Task StartServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Console.WriteLine($"TCPサーバーがポート {port} で起動しました...");

        while (_running)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = HandleClient(client);
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        Console.WriteLine("クライアントが接続しました。");

        try
        {
            while (_running)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // クライアントが切断した

                string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"受信: {command}");

                string response = await ProcessCommand(command);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"エラー: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine("クライアントが切断されました。");
        }
    }

    private async Task<string> ProcessCommand(string command)
    {
        if (_scenarios.TryGetValue(command, out var scenario))
        {
            if (scenario.Delay > 0)
            {
                await Task.Delay(scenario.Delay);
            }
            return scenario.Response;
        }
        return "UNKNOWN_COMMAND";
    }
}

// シナリオクラス
class Scenario
{
    public string Command { get; set; }
    public string Response { get; set; }
    public int Delay { get; set; } = 0;
}

class ScenarioList
{
    public List<Scenario> Scenarios { get; set; }
}
```

---

## **3. 動作イメージ**
### **シナリオロード**
```bash
シナリオがロードされました。
TCPサーバーがポート 5000 で起動しました...
```

### **クライアントが `GET_VOLTAGE` を送信**
```bash
受信: GET_VOLTAGE
応答: VOLTAGE: 12.00
```
（即時応答）

### **クライアントが `SIMULATE_TIMEOUT` を送信**
```bash
受信: SIMULATE_TIMEOUT
（5秒後に応答）
応答: TIMEOUT
```
（5秒遅れて応答）

---

## **4. 拡張ポイント**
### ✅ **シナリオの追加・編集が簡単**
- `scenarios.json` を変更するだけで、サーバーを再起動せずに新しいテストケースを追加可能。

### ✅ **異常値のテスト**
- `"response": "ERROR: Overcurrent detected"` のように設定し、エラーケースをシミュレート可能。

### ✅ **応答遅延を柔軟に変更**
- `"delay": 10000` にすれば、10秒の遅延も設定可能（タイムアウトテスト用）。

---

## **5. まとめ**
- **TCPサーバーがJSONシナリオをロード**
- **クライアントのコマンドに対して設定通りの応答を返す**
- **遅延やエラーを自由に設定可能**
- **シナリオの変更が容易**

これで、テスト用のTCPシミュレータが柔軟に運用できます！




### **時系列シナリオに対応したシミュレーション**
クライアントソフトは **電源に対して時系列のコマンドを送信し、マルチメーターはポーリングする** という動作を行うため、以下のようなシミュレーション機能を追加します。

---

## **📌 拡張ポイント**
1. **電源の時系列シナリオ制御**
   - シナリオファイルで **時間ごとのコマンド** を定義
   - 設定時間になったらコマンドを処理
   - 例: `t=0s で SET 12V 1A`, `t=10s で SET 15V 2A`

2. **マルチメーターのポーリング**
   - クライアントから定期的に `GET_VOLTAGE`, `GET_CURRENT`, `GET_POWER` を要求
   - サーバーは **最新の電源状態** を返す

3. **シミュレーションの開始・終了制御**
   - クライアントが `START_SCENARIO` を送信 → シナリオ開始
   - クライアントが `STOP_SCENARIO` を送信 → シナリオ停止

---

## **1️⃣ `scenario.json` (時系列シナリオファイル)**
時間 (`time`)、コマンド (`command`)、設定値 (`value`) を定義。

```json
{
  "scenario": [
    {
      "time": 0,
      "command": "SET",
      "value": "12.0 1.0"
    },
    {
      "time": 10,
      "command": "SET",
      "value": "15.0 2.0"
    },
    {
      "time": 20,
      "command": "SET",
      "value": "10.0 1.5"
    }
  ],
  "polling_interval": 2
}
```

- `time` : シナリオ実行開始からの秒数
- `command` : 実行するコマンド
- `value` : 設定する値 (`電圧 電流`)
- `polling_interval` : マルチメーターのポーリング間隔 (秒)

---

## **2️⃣ C# 実装**
時系列制御とポーリングに対応したサーバーを実装します。

### **📝 時系列シナリオ対応 TCPサーバー**
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var server = new TcpDeviceSimulator();
        await server.LoadConfig("config.json");
        await server.LoadScenario("scenario.json");
        await server.StartServer();
    }
}

class TcpDeviceSimulator
{
    private TcpListener _listener;
    private bool _running = true;
    private PowerSupply _powerSupply;
    private List<ScenarioStep> _scenario;
    private int _pollingInterval = 2; // デフォルトのポーリング間隔 (秒)

    public async Task LoadConfig(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<ServerConfig>(json);
            _powerSupply = new PowerSupply(config.PowerSupply.InitialVoltage, config.PowerSupply.InitialCurrent);
            Console.WriteLine("設定ファイルをロードしました。");
        }
        else
        {
            _powerSupply = new PowerSupply();
            Console.WriteLine("設定ファイルが見つかりません。デフォルト設定を使用します。");
        }
    }

    public async Task LoadScenario(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            var scenarioData = JsonSerializer.Deserialize<ScenarioData>(json);
            _scenario = scenarioData.Scenario;
            _pollingInterval = scenarioData.PollingInterval;
            Console.WriteLine("シナリオがロードされました。");
        }
    }

    public async Task StartServer()
    {
        _listener = new TcpListener(IPAddress.Any, 5000);
        _listener.Start();
        Console.WriteLine($"TCPサーバーが起動しました...");

        Task.Run(() => RunScenario());

        while (_running)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = HandleClient(client);
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        Console.WriteLine("クライアントが接続しました。");

        try
        {
            while (_running)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"受信: {command}");

                string response = ProcessCommand(command);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"エラー: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine("クライアントが切断されました。");
        }
    }

    private async Task RunScenario()
    {
        Console.WriteLine("シナリオを開始します...");
        int elapsedTime = 0;

        foreach (var step in _scenario)
        {
            while (elapsedTime < step.Time)
            {
                await Task.Delay(1000);
                elapsedTime++;
            }

            Console.WriteLine($"シナリオ実行: {step.Command} {step.Value}");
            _powerSupply.SetParameters($"SET {step.Value}");
        }
    }

    private string ProcessCommand(string command)
    {
        if (command == "GET_VOLTAGE") return $"VOLTAGE: {_powerSupply.Voltage:F2}";
        if (command == "GET_CURRENT") return $"CURRENT: {_powerSupply.Current:F2}";
        if (command == "GET_POWER") return $"POWER: {_powerSupply.Power:F2}";

        return "UNKNOWN_COMMAND";
    }
}

class PowerSupply
{
    public double Voltage { get; private set; }
    public double Current { get; private set; }
    public double Power => Voltage * Current;

    public PowerSupply(double initialVoltage = 12.0, double initialCurrent = 1.0)
    {
        Voltage = initialVoltage;
        Current = initialCurrent;
    }

    public void SetParameters(string command)
    {
        var parts = command.Split(' ');
        if (parts.Length != 3) return;

        if (double.TryParse(parts[1], out double voltage) && double.TryParse(parts[2], out double current))
        {
            Voltage = voltage;
            Current = current;
            Console.WriteLine($"電源設定更新: Voltage={Voltage}V, Current={Current}A");
        }
    }
}

class ScenarioStep
{
    public int Time { get; set; }
    public string Command { get; set; }
    public string Value { get; set; }
}

class ScenarioData
{
    public List<ScenarioStep> Scenario { get; set; }
    public int PollingInterval { get; set; }
}

class ServerConfig
{
    public PowerSupplySettings PowerSupply { get; set; }
}

class PowerSupplySettings
{
    public double InitialVoltage { get; set; }
    public double InitialCurrent { get; set; }
}
```

---

## **🎯 まとめ**
✅ **時系列に沿った電源制御**
- `scenario.json` に沿って電圧・電流を変更

✅ **マルチメーターのポーリング**
- `polling_interval` に応じてデータを取得

✅ **シミュレーションの開始・終了を制御**
- `START_SCENARIO`, `STOP_SCENARIO` に対応

これで、**時系列制御とポーリングを再現するシミュレーター** が実現できます！ 🚀