using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses.ProducerConsumer
{
    public class AsyncProcessor<T>
    {
        private readonly CustomChannel<T> _channel;
        private readonly Func<T, Task> _consumerAction;
        private readonly List<Task> _consumerTasks;
        private readonly CancellationTokenSource _cts;

        public AsyncProcessor(Func<T, Task> consumerAction, int capacity = 100, int consumerCount = 1)
        {
            _channel = new CustomChannel<T>(capacity);
            _consumerAction = consumerAction ?? throw new ArgumentNullException(nameof(consumerAction));
            _consumerTasks = new List<Task>();
            _cts = new CancellationTokenSource();

            for (int i = 0; i < consumerCount; i++)
            {
                _consumerTasks.Add(Task.Run(() => ConsumeAsync(_cts.Token)));
            }
        }

        // Producer (生産者)
        public async Task ProduceAsync(T item)
        {
            await _channel.WriteAsync(item);
        }

        // Consumer (消費者)
        private async Task ConsumeAsync(CancellationToken token)
        {
            try
            {
                while (!_channel.IsCompleted)
                {
                    var item = await _channel.ReadAsync(token);
                    await _consumerAction(item);
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセル時の処理
            }
        }

        // 終了処理
        public async Task ShutdownAsync()
        {
            _channel.Complete();
            _cts.Cancel();
            await Task.WhenAll(_consumerTasks);
        }
    }

    public partial class Program
    {
        public async Task a()
        {
            string imageFolder = @"C:\Images"; // 読み込むフォルダ

            var imageProcessor = new AsyncProcessor<string>(async filePath =>
            {
                using (var image = Image.FromFile(filePath)) // 画像を読み込む
                {
                    Console.WriteLine($"Processed Image: {filePath}, Size: {image.Width}x{image.Height}");
                }
                await Task.Delay(200); // 模擬的な処理時間
            }, consumerCount: 3); // 並列処理数を3に設定

            // 画像ファイルを Producer に追加
            foreach (var file in Directory.GetFiles(imageFolder, "*.jpg"))
            {
                await imageProcessor.ProduceAsync(file);
            }

            // シャットダウン
            await imageProcessor.ShutdownAsync();
        }
    }
}
