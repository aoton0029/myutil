using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses.Backgrounds
{
    public class BackgroundDataProcessor : BackgroundService
    {
        private readonly Channel<DataWithKey> _channel = new();
        private readonly ConcurrentDictionary<string, KeySpecificDataProcessor> _dataProcessors = new();
        private readonly BackgroundDataProcessorMonitor _monitor;
        private readonly object _processorsLock = new();

        public BackgroundDataProcessor()
        {
            _monitor = new BackgroundDataProcessorMonitor(_dataProcessors);
        }

        public void ScheduleProcessingData(DataWithKey data)
        {
            _channel.Write(data);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _monitor.StartMonitoring();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var data = _channel.Read(stoppingToken);

                    lock (_processorsLock)
                    {
                        if (!_dataProcessors.TryGetValue(data.ProcessorKey, out var processor))
                        {
                            processor = new KeySpecificDataProcessor(data.ProcessorKey);
                            _dataProcessors[data.ProcessorKey] = processor;
                        }

                        processor.ProcessData(data.Data);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _monitor.StopMonitoring();
        }
    }
}
