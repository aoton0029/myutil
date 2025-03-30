using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsyncSample
{
    public record DataWithKey(int Key, string Data);

    public interface IDataProcessor
    {
        Task ScheduleDataProcessing(DataWithKey dataWithKey);
    }

    public class BackgroundDataProcessor : IDataProcessor
    {
        private readonly ConcurrentQueue<DataWithKey> _internalQueue = new();
        private readonly object _processorsLock = new();
        private readonly Dictionary<int, KeySpecificDataProcessor> _dataProcessors = new();

        private BackgroundDataProcessorMonitor? _monitor;

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_internalQueue.TryDequeue(out var dataWithKey))
                {
                    KeySpecificDataProcessor processor;

                    lock (_processorsLock)
                    {
                        if (!_dataProcessors.TryGetValue(dataWithKey.Key, out processor))
                        {
                            processor = new KeySpecificDataProcessor(dataWithKey.Key);
                            _dataProcessors[dataWithKey.Key] = processor;
                            processor.StartProcessing();
                        }
                    }

                    await processor.ScheduleDataProcessing(dataWithKey);
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
        }

        public Task ScheduleDataProcessing(DataWithKey dataWithKey)
        {
            _internalQueue.Enqueue(dataWithKey);
            return Task.CompletedTask;
        }
    }

    public class BackgroundDataProcessorMonitor
    {
        private readonly TimeSpan _processorExpiryScanningPeriod;
        private readonly TimeSpan _processorExpiryThreshold;
        private readonly object _processorsLock;
        private readonly Dictionary<int, KeySpecificDataProcessor> _dataProcessors;

        private Task _monitoringTask;
        private CancellationTokenSource _cts;

        public BackgroundDataProcessorMonitor(
            TimeSpan scanningPeriod,
            TimeSpan expiryThreshold,
            object processorsLock,
            Dictionary<int, KeySpecificDataProcessor> dataProcessors)
        {
            _processorExpiryScanningPeriod = scanningPeriod;
            _processorExpiryThreshold = expiryThreshold;
            _processorsLock = processorsLock;
            _dataProcessors = dataProcessors;
        }

        public void StartMonitoring()
        {
            _cts = new CancellationTokenSource();
            _monitoringTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(_processorExpiryScanningPeriod, _cts.Token);

                    lock (_processorsLock)
                    {
                        var now = DateTime.UtcNow;
                        var expiredKeys = _dataProcessors
                            .Where(kvp => IsExpired(kvp.Value, now))
                            .Select(kvp => kvp.Key)
                            .ToList();

                        foreach (var key in expiredKeys)
                        {
                            _dataProcessors[key].StopProcessing();
                            _dataProcessors.Remove(key);
                        }
                    }
                }
            });
        }

        public void StopMonitoring()
        {
            _cts?.Cancel();
        }

        private bool IsExpired(KeySpecificDataProcessor processor, DateTime now)
        {
            return (now - processor.LastProcessingTimestamp) > _processorExpiryThreshold;
        }
    }


    public class KeySpecificDataProcessor : IDataProcessor
    {
        public int Key { get; }
        public DateTime LastProcessingTimestamp { get; private set; }

        private Task _processingTask;
        private readonly ConcurrentQueue<DataWithKey> _internalQueue = new();

        private CancellationTokenSource _cts;

        public KeySpecificDataProcessor(int key)
        {
            Key = key;
        }

        public Task ScheduleDataProcessing(DataWithKey dataWithKey)
        {
            _internalQueue.Enqueue(dataWithKey);
            return Task.CompletedTask;
        }

        public void StartProcessing()
        {
            _cts = new CancellationTokenSource();
            _processingTask = Task.Factory.StartNew(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    if (_internalQueue.TryDequeue(out var data))
                    {
                        await ProcessData(data);
                    }
                    else
                    {
                        await Task.Delay(100, _cts.Token); // idle wait
                    }
                }
            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }

        public void StopProcessing()
        {
            _cts?.Cancel();
        }

        private async Task ProcessData(DataWithKey data)
        {
            // 処理内容をここに記述
            LastProcessingTimestamp = DateTime.UtcNow;
            await Task.Delay(100); // ダミー処理
        }
    }

    public static class SemaphoreSlimExtensions
    {
        public static async Task<bool> WaitWithCancellation(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            try
            {
                await semaphore.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }
    }
}
