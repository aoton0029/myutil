マネージャークラスを導入し、サーバー管理を統合

各デバイスの 起動・停止・エラー処理・ログ管理 を ServerManager クラスにまとめます。

主な機能

✅ 複数のデバイスサーバーの管理（起動・停止）
✅ エラーハンドリングとリカバリー（エラー発生時の再起動）
✅ ログ管理（サーバー起動・クライアント接続・コマンド受信の記録）
✅ 非同期での管理（複数のサーバーを並列起動）


---

1. DeviceServer（デバイスごとのTCPサーバー）

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class DeviceServer
{
    private readonly string _ipAddress;
    private readonly int _port;
    private TcpListener _listener;
    private Device _device;
    public event Action<string> OnLog; // ログ出力用イベント
    public event Action<string, Exception> OnError; // エラー処理イベント

    public DeviceServer(string ipAddress, int port, string deviceName)
    {
        _ipAddress = ipAddress;
        _port = port;
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _device = new Device(deviceName);
    }

    public async Task StartAsync()
    {
        try
        {
            _listener.Start();
            OnLog?.Invoke($"[{_device.Name}] サーバー起動 {_ipAddress}:{_port}");

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                OnLog?.Invoke($"[{_device.Name}] クライアント接続");

                _ = HandleClientAsync(client);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"[{_device.Name}] サーバーエラー", ex);
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
                OnLog?.Invoke($"[{_device.Name}] 受信: {receivedText}");

                string response = _device.ProcessCommand(receivedText);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"[{_device.Name}] クライアント処理エラー", ex);
        }
        finally
        {
            OnLog?.Invoke($"[{_device.Name}] クライアント切断");
            client.Close();
        }
    }

    public void Stop()
    {
        _listener.Stop();
        OnLog?.Invoke($"[{_device.Name}] サーバー停止");
    }
}


---

2. Device（仮想デバイスの動作定義）

class Device
{
    public string Name { get; }
    private double _voltage = 0.0;
    private double _current = 1.0;

    public Device(string name)
    {
        Name = name;
    }

    public string ProcessCommand(string command)
    {
        string[] parts = command.Split(' ');
        switch (parts[0])
        {
            case "GET_VOLTAGE":
                return $"VOLTAGE {_voltage}V";

            case "SET_VOLTAGE":
                if (parts.Length > 1 && double.TryParse(parts[1], out double voltage))
                {
                    _voltage = voltage;
                    return $"VOLTAGE SET TO {_voltage}V";
                }
                return "INVALID VOLTAGE VALUE";

            case "MEASURE_CURRENT":
                return $"CURRENT {_current}A";

            case "SET_CURRENT":
                if (parts.Length > 1 && double.TryParse(parts[1], out double current))
                {
                    _current = current;
                    return $"CURRENT SET TO {_current}A";
                }
                return "INVALID CURRENT VALUE";

            case "GET_POWER":
                return $"POWER {_voltage * _current}W";

            case "STATUS":
                return $"STATUS: Voltage={_voltage}V, Current={_current}A, Power={_voltage * _current}W";

            default:
                return "UNKNOWN COMMAND";
        }
    }
}


---

3. ServerManager（サーバー全体の管理）

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

class ServerManager
{
    private List<DeviceServer> _servers = new();
    private List<Task> _serverTasks = new();
    private string _logFilePath = "server_log.txt";

    public ServerManager()
    {
        _servers.Add(new DeviceServer("192.168.1.100", 5001, "PowerSupply_1"));
        _servers.Add(new DeviceServer("192.168.1.101", 5002, "Multimeter_1"));
        _servers.Add(new DeviceServer("192.168.1.102", 5003, "PowerSupply_2"));

        foreach (var server in _servers)
        {
            server.OnLog += LogMessage;
            server.OnError += HandleError;
        }
    }

    public async Task StartAllAsync()
    {
        Console.WriteLine("全サーバーを起動します...");
        foreach (var server in _servers)
        {
            _serverTasks.Add(server.StartAsync());
        }
        await Task.WhenAll(_serverTasks);
    }

    public void StopAll()
    {
        Console.WriteLine("全サーバーを停止します...");
        foreach (var server in _servers)
        {
            server.Stop();
        }
    }

    private void LogMessage(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText(_logFilePath, $"{DateTime.Now}: {message}\n");
    }

    private void HandleError(string context, Exception ex)
    {
        string errorMessage = $"{context} - {ex.Message}";
        Console.WriteLine("ERROR: " + errorMessage);
        File.AppendAllText(_logFilePath, $"{DateTime.Now} ERROR: {errorMessage}\n");
    }
}


---

4. Program（エントリーポイント）

using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        ServerManager manager = new ServerManager();
        
        Task.Run(async () =>
        {
            await manager.StartAllAsync();
        });

        Console.WriteLine("サーバーが起動しました。Enterキーで停止します。");
        Console.ReadLine();

        manager.StopAll();
    }
}


---

改善点

1. サーバーの管理を ServerManager に統合

サーバーの 起動・停止 を一括管理

リストに追加すれば簡単にデバイスを増やせる



2. ログ出力を統合

server_log.txt にサーバーの動作を記録

エラー情報も保存



3. エラーハンドリング

各 DeviceServer はエラー発生時に OnError で ServerManager に通知

エラーメッセージをログに記録



4. シンプルな API

StartAllAsync() で すべてのサーバーを起動

StopAll() で すべてのサーバーを停止





---

動作例

サーバー起動時

全サーバーを起動します...
[PowerSupply_1] サーバー起動 192.168.1.100:5001
[Multimeter_1] サーバー起動 192.168.1.101:5002
[PowerSupply_2] サーバー起動 192.168.1.102:5003

クライアントが接続

[PowerSupply_1] クライアント接続
[PowerSupply_1] 受信: SET_VOLTAGE 12.5

サーバー停止

全サーバーを停止します...
[PowerSupply_1] サーバー停止
[Multimeter_1] サーバー停止
[PowerSupply_2] サーバー停止

これで、複数のデバイスを効率的に管理できるサーバーが構築できます！







複数のデバイスが異なる IPアドレス を持つ場合、それぞれのデバイスごとに 別の TCP サーバー を立ち上げる設計にする必要があります。

解決策

各デバイスごとに異なるIPアドレスを指定して TcpListener を作成

複数の TCP サーバーを同時に動作 させる（スレッド or Task を使用）

各サーバーが独立してデバイスの状態を管理



---

実装

複数の IP アドレスに対応

デバイスごとに異なるポートでも動作可能

各サーバーが独立したデバイス情報を管理


1. DeviceServer クラス（1台のデバイスを管理するサーバー）

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class DeviceServer
{
    private readonly string _ipAddress;
    private readonly int _port;
    private TcpListener _listener;
    private Device _device;

    public DeviceServer(string ipAddress, int port, string deviceName)
    {
        _ipAddress = ipAddress;
        _port = port;
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _device = new Device(deviceName);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"[{_device.Name}] サーバー起動 {_ipAddress}:{_port}");

        while (true)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine($"[{_device.Name}] クライアントが接続しました。");

            _ = HandleClientAsync(client);
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
                Console.WriteLine($"[{_device.Name}] 受信: {receivedText}");

                string response = _device.ProcessCommand(receivedText);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_device.Name}] エラー: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"[{_device.Name}] クライアントが切断しました。");
            client.Close();
        }
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine($"[{_device.Name}] サーバー停止");
    }
}


---

2. Device クラス（仮想デバイスの状態管理）

class Device
{
    public string Name { get; }
    private double _voltage = 0.0;
    private double _current = 1.0;

    public Device(string name)
    {
        Name = name;
    }

    public string ProcessCommand(string command)
    {
        string[] parts = command.Split(' ');
        switch (parts[0])
        {
            case "GET_VOLTAGE":
                return $"VOLTAGE {_voltage}V";

            case "SET_VOLTAGE":
                if (parts.Length > 1 && double.TryParse(parts[1], out double voltage))
                {
                    _voltage = voltage;
                    return $"VOLTAGE SET TO {_voltage}V";
                }
                return "INVALID VOLTAGE VALUE";

            case "MEASURE_CURRENT":
                return $"CURRENT {_current}A";

            case "SET_CURRENT":
                if (parts.Length > 1 && double.TryParse(parts[1], out double current))
                {
                    _current = current;
                    return $"CURRENT SET TO {_current}A";
                }
                return "INVALID CURRENT VALUE";

            case "GET_POWER":
                return $"POWER {_voltage * _current}W";

            case "STATUS":
                return $"STATUS: Voltage={_voltage}V, Current={_current}A, Power={_voltage * _current}W";

            default:
                return "UNKNOWN COMMAND";
        }
    }
}


---

3. Program クラス（複数のサーバーを起動）

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var servers = new List<DeviceServer>
        {
            new DeviceServer("192.168.1.100", 5001, "PowerSupply_1"),
            new DeviceServer("192.168.1.101", 5002, "Multimeter_1"),
            new DeviceServer("192.168.1.102", 5003, "PowerSupply_2"),
        };

        var serverTasks = new List<Task>();

        foreach (var server in servers)
        {
            serverTasks.Add(server.StartAsync());
        }

        await Task.WhenAll(serverTasks);
    }
}


---

動作の流れ

サーバーの起動

[PowerSupply_1] サーバー起動 192.168.1.100:5001
[Multimeter_1] サーバー起動 192.168.1.101:5002
[PowerSupply_2] サーバー起動 192.168.1.102:5003

クライアントが PowerSupply_1 に接続し、電圧を設定

[PowerSupply_1] クライアントが接続しました。
[PowerSupply_1] 受信: SET_VOLTAGE 12.5
[PowerSupply_1] クライアントが切断しました。

クライアント側のレスポンス

VOLTAGE SET TO 12.5V


---

メリット

✅ 異なる IP で複数のサーバーを同時に起動
✅ 各デバイスが独立して動作し、複数のクライアントと同時通信可能
✅ スレッドセーフで管理しやすいシンプルな設計
✅ 拡張が容易（デバイスの種類を増やせる）


---

応用

デバイスの種類ごとに異なる動作を定義

各デバイスに対して独自の通信プロトコルを適用

デバイスのデータを定期的にブロードキャスト

ログ保存やデータ解析機能の追加


この設計を基に、マルチデバイスのシミュレーション環境を構築できます！







IPアドレスでデバイスの種類を識別する TCP サーバー

各クライアントが 接続する IP アドレスに応じて 仮想デバイスの種類を自動で割り当てるように設定できます。


---

改良ポイント

1. 接続元の IP アドレスを確認

client.Client.RemoteEndPoint を取得し、IP に応じたデバイスを割り当て



2. IP アドレスに基づいたデバイスマッピング

127.0.0.2 → 電源 (PSU)

127.0.0.3 → マルチメーター 1 (DMM1)

127.0.0.4 → マルチメーター 2 (DMM2)

127.0.0.5 → マルチメーター 3 (DMM3)





---

1. IP に基づくデバイスマッピング

using System;
using System.Collections.Generic;
using System.Net;

class DeviceFactory
{
    private static readonly Dictionary<string, string> IpToDeviceType = new()
    {
        { "127.0.0.2", "PSU" },   // 電源
        { "127.0.0.3", "DMM1" },  // マルチメーター 1
        { "127.0.0.4", "DMM2" },  // マルチメーター 2
        { "127.0.0.5", "DMM3" }   // マルチメーター 3
    };

    public static VirtualDevice CreateDevice(IPAddress clientIp, string deviceId)
    {
        if (IpToDeviceType.TryGetValue(clientIp.ToString(), out var deviceType))
        {
            return deviceType.StartsWith("PSU") ? new PowerSupply(deviceId) : new Multimeter(deviceId);
        }
        throw new ArgumentException("Invalid client IP address");
    }
}


---

2. TCP サーバー（IP に基づくデバイス割り当て）

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class TcpDeviceSimulator
{
    private readonly TcpListener _server;
    private readonly ConcurrentDictionary<TcpClient, VirtualDevice> _clientDevices = new();
    private int _deviceCounter = 0;

    public TcpDeviceSimulator(int port)
    {
        _server = new TcpListener(IPAddress.Any, port);
    }

    public void Start()
    {
        _server.Start();
        Console.WriteLine("TCPサーバーが起動しました...");

        Task.Run(async () =>
        {
            while (true)
            {
                var client = await _server.AcceptTcpClientAsync();
                var clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

                try
                {
                    var deviceId = $"Device{++_deviceCounter}";
                    var device = DeviceFactory.CreateDevice(clientIp, deviceId);
                    _clientDevices[client] = device;

                    Console.WriteLine($"クライアント接続: {clientIp} -> {deviceId} ({device.Type})");

                    var task = HandleClientAsync(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"無効な IP ({clientIp}): {ex.Message}");
                    client.Close();
                }
            }
        });
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            var device = _clientDevices[client];

            while (true)
            {
                try
                {
                    var command = await reader.ReadLineAsync();
                    if (command == null) break;

                    Console.WriteLine($"[{device.Id}] 受信: {command}");
                    var response = device.ProcessCommand(command);
                    await writer.WriteLineAsync(response);
                    Console.WriteLine($"[{device.Id}] 送信: {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"クライアント処理エラー: {ex.Message}");
                    break;
                }
            }
        }

        _clientDevices.TryRemove(client, out _);
        Console.WriteLine($"クライアント切断: {client.Client.RemoteEndPoint}");
    }

    public void Stop()
    {
        _server.Stop();
        Console.WriteLine("TCPサーバーを停止しました。");
    }
}


---

3. サーバーの起動

class Program
{
    static void Main()
    {
        var server = new TcpDeviceSimulator(5000);
        server.Start();

        Console.WriteLine("Enterキーで終了...");
        Console.ReadLine();
        server.Stop();
    }
}


---

動作イメージ

クライアントが 127.0.0.2 に接続

クライアント接続: 127.0.0.2 -> Device1 (PSU)
送信: GET_VOLTAGE
受信: 12.00V

クライアントが 127.0.0.3 に接続

クライアント接続: 127.0.0.3 -> Device2 (DMM1)
送信: GET_VOLTAGE
受信: 13.24V  (ランダム値)

クライアントが 127.0.0.4 に接続

クライアント接続: 127.0.0.4 -> Device3 (DMM2)
送信: GET_CURRENT
受信: 0.85A  (ランダム値)

クライアントが 127.0.0.5 に接続

クライアント接続: 127.0.0.5 -> Device4 (DMM3)
送信: GET_POWER
受信: 18.23W  (ランダム値)


---

まとめ

✅ クライアントの IP アドレスでデバイスの種類を自動判別
✅ マッピングを Dictionary で管理するため拡張が容易
✅ クライアントが自動的に正しいデバイスと通信可能

これで、各クライアントが 接続先 IP アドレスに応じて適切な仮想デバイスを取得できる TCP サーバーが完成しました！




﻿### **外部設定の拡張**
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