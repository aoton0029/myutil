using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses.Backgrounds
{
    public class BackgroundDataProcessorMonitor : BackgroundService
    {
        private readonly ConcurrentDictionary<string, KeySpecificDataProcessor> _dataProcessors;
        private readonly TimeSpan _processorExpiryThreshold = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _processorExpiryScanningPeriod = TimeSpan.FromSeconds(30);
        private Task? _monitoringTask;
        private readonly object _processorsLock = new();

        public BackgroundDataProcessorMonitor(ConcurrentDictionary<string, KeySpecificDataProcessor> dataProcessors)
        {
            _dataProcessors = dataProcessors;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _monitoringTask = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        ScanAndRemoveExpiredProcessors();
                        await Task.Delay(_processorExpiryScanningPeriod, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, stoppingToken);
        }

        private void ScanAndRemoveExpiredProcessors()
        {
            lock (_processorsLock)
            {
                var now = DateTime.UtcNow;
                foreach (var key in _dataProcessors.Keys)
                {
                    if (_dataProcessors.TryGetValue(key, out var processor) && IsExpired(processor))
                    {
                        _dataProcessors.TryRemove(key, out _);
                        Console.WriteLine($"Removed expired processor: {key}");
                    }
                }
            }
        }

        private bool IsExpired(KeySpecificDataProcessor processor)
        {
            return (DateTime.UtcNow - processor.LastProcessingTimestamp) > _processorExpiryThreshold;
        }

        public void StartMonitoring() => StartProcessing();
        public void StopMonitoring() => StopProcessing();
    }
}
