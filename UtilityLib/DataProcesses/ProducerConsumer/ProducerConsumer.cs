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

    class Program
    {
        public void Main()
        {
            string filePath = "data.jsonl";

            var processor = new ProducerConsumer<string>(
                maxQueueSize: 100,
                consumerCount: 4,
                processItem: ProcessJson
            );

            // JSONLを1行ずつProducerに渡す
            processor.Produce(ReadLines(filePath));
            processor.StartConsumers();
            processor.WaitForCompletion();

            Console.WriteLine("JSONL processing complete.");

            /**************************************************/
            string folderPath = "images";

            var processor2 = new ProducerConsumer<string>(
                maxQueueSize: 10,
                consumerCount: 4,
                processItem: ProcessImage
            );

            // フォルダ内の画像ファイルをProducerに渡す
            processor2.Produce(Directory.GetFiles(folderPath, "*.jpg"));
            processor2.StartConsumers();
            processor2.WaitForCompletion();

            Console.WriteLine("Image processing complete.");
        }

        void ProcessImage(string filePath)
        {
            using (var img = Image.FromFile(filePath))
            {
                Console.WriteLine($"Processing Image: {filePath}");
                Thread.Sleep(200); // ダミー処理
            }
        }

        IEnumerable<string> ReadLines(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        void ProcessJson(string json)
        {
            Console.WriteLine($"Processing JSON: {json}");
            Thread.Sleep(100); // ダミー処理
        }
    }
}
