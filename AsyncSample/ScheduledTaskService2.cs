using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSample
{
    public record DataWithKey(int Key, string Data);

    public class KeySpecificTask
    {
        Task ScheduleDataProcessing(DataWithKey dataWithKey)
        {

        }
    }

    public class BackgroundDataService : IDisposable
    {
        private readonly ConcurrentQueue<DataWithKey> _internalQueue;
        private readonly Dictionary<int, KeySpecificTask> _dataProcessors = new();
        private readonly SemaphoreSlim _processorsLock = new(1, 1);

        public BackgroundDataService()
        {

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // モニタ開始
            while ()
            {

            }
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

            // モニタ停止
        }

        public void ScheduleDataProcessing(DataWithKey dataWithKey)
        {
            _internalQueue.Enqueue(dataWithKey);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}
