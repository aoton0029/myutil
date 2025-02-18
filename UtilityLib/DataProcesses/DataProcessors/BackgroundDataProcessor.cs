using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses
{
    public record DataWithKey(int Key, string Data);

    public partial class BackgroundDataProcessor : BackgroundService, IDataProcessor
    {
        private readonly Channel<DataWithKey> _internalQueue = Channel.CreateUnbounded<DataWithKey>(new UnboundedChannelOptions { SingleReader = true });

        private readonly Dictionary<int, KeySpecificDataProcessor> _dataProcessors = new();

        private readonly SemaphoreSlim _processorsLock = new(1, 1);

        private BackgroundDataProcessorMonitor? _monitor;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ILoggerFactory _loggerFactory;

        private readonly ILogger<BackgroundDataProcessor> _logger;

        public BackgroundDataProcessor(IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<BackgroundDataProcessor>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _monitor = BackgroundDataProcessorMonitor.CreateAndStartMonitoring(_processorsLock, _dataProcessors, _loggerFactory.CreateLogger<BackgroundDataProcessorMonitor>(), stoppingToken);
            await foreach (var data in _internalQueue.Reader.ReadAllAsync(stoppingToken))
            {
                if (!await _processorsLock.WaitWithCancellation(stoppingToken))
                {
                    break;
                }

                var processor = GetOrCreateDataProcessor(data, stoppingToken);
                await processor.ScheduleDataProcessing(data);

                _processorsLock.Release();
                _logger.LogInformation("Scheduled new data '{Data}' for processor with key '{Key}'", data, data.Key);
            }

            await _monitor.StopMonitoring();
        }

        private KeySpecificDataProcessor GetOrCreateDataProcessor(DataWithKey dataWithKey, CancellationToken newProcessorCancellationToken = default)
        {
            if (!_dataProcessors.TryGetValue(dataWithKey.Key, out var deviceProcessor))
            {
                var processor = CreateNewProcessor(dataWithKey.Key, newProcessorCancellationToken);
                _dataProcessors[dataWithKey.Key] = processor;
                deviceProcessor = processor;
                _logger.LogInformation("Created new processor for data key: {Key}", dataWithKey.Key);
            }

            return deviceProcessor;
        }

        private KeySpecificDataProcessor CreateNewProcessor(int dataKey, CancellationToken processorCancellationToken = default)
        {
            var logger = _loggerFactory.CreateLogger($"{typeof(KeySpecificDataProcessor).FullName}-{dataKey}");
            return KeySpecificDataProcessor.CreateAndStartProcessing(dataKey, _serviceScopeFactory, logger, processorCancellationToken);
        }

        public async Task ScheduleDataProcessing(DataWithKey dataWithKey) => await _internalQueue.Writer.WriteAsync(dataWithKey);
    }

    public partial class BackgroundDataProcessor
    {
        public class BackgroundDataProcessorMonitor
        {
            private readonly TimeSpan _processorExpiryThreshold = TimeSpan.FromSeconds(30);

            private readonly TimeSpan _processorExpiryScanningPeriod = TimeSpan.FromSeconds(5);

            private MonitoringTask? _monitoringTask;

            private readonly SemaphoreSlim _processorsLock;

            private readonly Dictionary<int, KeySpecificDataProcessor> _dataProcessors;

            private readonly ILogger<BackgroundDataProcessorMonitor> _logger;

            private BackgroundDataProcessorMonitor(SemaphoreSlim processorsLock, Dictionary<int, KeySpecificDataProcessor> dataProcessors, ILogger<BackgroundDataProcessorMonitor> logger)
            {
                _processorsLock = processorsLock;
                _dataProcessors = dataProcessors;
                _logger = logger;
            }

            private void StartMonitoring(CancellationToken cancellationToken = default)
            {
                var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var task = Task.Factory.StartNew(async () =>
                {
                    using var timer = new PeriodicTimer(_processorExpiryScanningPeriod);
                    while (!tokenSource.IsCancellationRequested && await timer.WaitForNextTickAsync(tokenSource.Token))
                    {
                        if (!await _processorsLock.WaitWithCancellation(tokenSource.Token))
                        {
                            continue;
                        }

                        var expiredProcessors = _dataProcessors.Values.Where(IsExpired).ToArray();
                        foreach (var expiredProcessor in expiredProcessors)
                        {
                            await expiredProcessor.StopProcessing();
                            _dataProcessors.Remove(expiredProcessor.ProcessorKey);

                            _logger.LogInformation("Removed data processor for data key {Key}", expiredProcessor.ProcessorKey);
                        }

                        _processorsLock.Release();
                    }
                }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _monitoringTask = new MonitoringTask(task, tokenSource);
            }

            private bool IsExpired(KeySpecificDataProcessor processorInfo) => (DateTime.UtcNow - processorInfo.LastProcessingTimestamp) > _processorExpiryThreshold;

            public async Task StopMonitoring()
            {
                if (_monitoringTask.HasValue)
                {
                    if (!_monitoringTask.Value.CancellationTokenSource.IsCancellationRequested)
                    {
                        _monitoringTask.Value.CancellationTokenSource.Cancel();
                    }

                    await _monitoringTask.Value.Task;
                    _monitoringTask.Value.CancellationTokenSource.Dispose();
                    _monitoringTask = null;
                }
            }

            public static BackgroundDataProcessorMonitor CreateAndStartMonitoring(SemaphoreSlim processorsLock, Dictionary<int, KeySpecificDataProcessor> dataProcessors, ILogger<BackgroundDataProcessorMonitor> logger, CancellationToken monitoringCancellationToken = default)
            {
                var monitor = new BackgroundDataProcessorMonitor(processorsLock, dataProcessors, logger);
                monitor.StartMonitoring(monitoringCancellationToken);
                return monitor;
            }

            private readonly record struct MonitoringTask(Task Task, CancellationTokenSource CancellationTokenSource);
        }
    }
}
