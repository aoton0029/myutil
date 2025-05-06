using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses.ProducerConsumer
{
    public class ProducerConsumer<T>
    {
        private readonly BlockingCollection<T> queue;
        private readonly List<Task> consumerTasks;
        private readonly int consumerCount;
        private readonly Action<T> processItem;

        public ProducerConsumer(int maxQueueSize, int consumerCount, Action<T> processItem)
        {
            this.queue = new BlockingCollection<T>(boundedCapacity: maxQueueSize);
            this.consumerTasks = new List<Task>();
            this.consumerCount = consumerCount;
            this.processItem = processItem;
        }

        // Producer: データをキューに追加
        public void Produce(IEnumerable<T> items)
        {
            Task.Run(() =>
            {
                foreach (var item in items)
                {
                    queue.Add(item);
                }
                queue.CompleteAdding(); // 生産完了
            });
        }

        // Consumer: データを処理
        public void StartConsumers()
        {
            for (int i = 0; i < consumerCount; i++)
            {
                consumerTasks.Add(Task.Run(() =>
                {
                    foreach (var item in queue.GetConsumingEnumerable())
                    {
                        processItem(item);
                    }
                }));
            }
        }

        // すべてのConsumerの終了を待機
        public void WaitForCompletion()
        {
            Task.WaitAll(consumerTasks.ToArray());
        }
    }

    
}
