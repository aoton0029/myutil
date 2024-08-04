using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Workers.WorkerQueue
{
    public sealed class WorkerQueueService : BackgroundService
    {
        private readonly IWorkItemQueue queue;
        private readonly Dictionary<Guid, int> workItemAttempts = new();

        public WorkerQueueService(IWorkItemQueue queue)
        {
            this.queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // TODO: Multi-thread
                //const int parallelWorkCount = 3; // TODO move to appsettings.
                //var itemsToGrab = parallelWorkCount > queue.Count ? queue.Count : parallelWorkCount;

                var workItem = await GetNextItem(stoppingToken);

                var result = await DoWork(workItem, stoppingToken);

                var attemptCount = workItemAttempts.GetValueOrDefault(workItem.Id);
                await RetryIfNeeded(stoppingToken, result, attemptCount, workItem);

            }

        }

        private async Task RetryIfNeeded(CancellationToken stoppingToken, WorkResult result, int attemptCount, WorkItem workItem)
        {
            if (result.Status != WorkResult.Statuses.Successful)
            {
                const int max = 3; // TODO move to appsettings.
                if (attemptCount >= max)
                {
                    workItem.Cancel();
                    workItemAttempts.Remove(workItem.Id);
                }
                else
                {
                    var newWorkItem = workItem.Clone();
                    await QueueItemForRetryAsync(newWorkItem, attemptCount, stoppingToken);
                }
            }
        }

        private static async Task<WorkResult> DoWork(WorkItem workItem, CancellationToken stoppingToken)
        {
            var stopwatch = Stopwatch.StartNew();
            WorkResult result;
            Task? delayTask = null;

            try
            {
                var workTokenSource = new CancellationTokenSource();
                var workTask = workItem.DoWorkAsync(workTokenSource.Token);
                const int workTtlMs = 10000; // TODO move to appsettings.
                delayTask = Task.Delay(workTtlMs, stoppingToken);
                var completedTask = await Task.WhenAny(workTask, delayTask);
                stopwatch.Stop();

                if (completedTask == workTask)
                {
                    result = workTask.Result;
                }
                else
                {
                    await workTokenSource.CancelAsync();
                    result = stoppingToken.IsCancellationRequested
                        ? WorkResult.Cancelled(stopwatch.ElapsedMilliseconds, "Cancelled by service.")
                        : WorkResult.TimedOut(stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception exception)
            {
                result = WorkResult.Failed(stopwatch.ElapsedMilliseconds, exception);
            }
            finally
            {
                stopwatch.Stop();
                delayTask?.Dispose();
            }

            return result;
        }

        private async Task<WorkItem> GetNextItem(CancellationToken stoppingToken)
        {
            var workItem = await queue.PopAsync(stoppingToken).AsTask();

            if (stoppingToken.IsCancellationRequested)
            {
                var attempts = workItemAttempts.GetValueOrDefault(workItem.Id);
                await QueueItemForRetryAsync(workItem, attempts, stoppingToken);
                stoppingToken.ThrowIfCancellationRequested();
            }

            return workItem;
        }

        private async Task QueueItemForRetryAsync(WorkItem workItem, int attemptCount, CancellationToken stoppingToken)
        {
            workItemAttempts[workItem.Id] = ++attemptCount;
            await queue.PushAsync(workItem, stoppingToken);
        }
    }
}
