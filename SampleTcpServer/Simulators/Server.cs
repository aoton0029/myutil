using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpServer.Simulators
{
    class ServerManager
    {
        private readonly List<DeviceServer> _servers = new();
        private readonly List<Task> _serverTasks = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly AsyncLogProcessor _logProcessor;

        public ServerManager(AsyncLogProcessor logProcessor)
        {
            _logProcessor = logProcessor;

            _servers.Add(new DeviceServer("127.0.0.2", 5001, new PowerSupply("PowerSupply_1"), new PowerSupplyProtocol(), _logProcessor));
            _servers.Add(new DeviceServer("127.0.0.3", 5002, new Multimeter("Multimeter_1"), new MultimeterProtocol(), _logProcessor));
            _servers.Add(new DeviceServer("127.0.0.4", 5003, new TemperatureSensor("TemperatureSensor_1"), new TemperatureSensorProtocol(), _logProcessor));
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
}

