C#での非同期データプロセッサーパターン

非同期データプロセッサーパターンは、データの処理を非同期に行い、システムのスループットや応答性を向上させるデザインパターンの一種です。C# では Task や Channel<T> を活用して実装できます。


---

1. 基本的な実装

Channel<T> を使用した非同期データプロセッサ

System.Threading.Channels を利用すると、スレッド間で非同期にデータをやり取りしながら処理を行うことができます。

実装例

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

class AsyncDataProcessor<T>
{
    private readonly Channel<T> _channel;
    private readonly List<Task> _workers;
    private readonly CancellationTokenSource _cts;
    private readonly Func<T, Task> _processItemAsync;

    public AsyncDataProcessor(int workerCount, Func<T, Task> processItemAsync)
    {
        _channel = Channel.CreateUnbounded<T>();
        _workers = new List<Task>();
        _cts = new CancellationTokenSource();
        _processItemAsync = processItemAsync;

        for (int i = 0; i < workerCount; i++)
        {
            _workers.Add(Task.Run(() => ProcessItemsAsync(_cts.Token)));
        }
    }

    public async Task EnqueueAsync(T item)
    {
        await _channel.Writer.WriteAsync(item);
    }

    private async Task ProcessItemsAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await _processItemAsync(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing item: {ex}");
            }
        }
    }

    public async Task StopAsync()
    {
        _channel.Writer.Complete();
        _cts.Cancel();
        await Task.WhenAll(_workers);
    }
}

// 使用例
class Program
{
    static async Task Main()
    {
        var processor = new AsyncDataProcessor<int>(3, async item =>
        {
            Console.WriteLine($"Processing: {item}");
            await Task.Delay(500); // 模擬的な処理時間
        });

        for (int i = 0; i < 10; i++)
        {
            await processor.EnqueueAsync(i);
        }

        await Task.Delay(3000); // 処理が終わるのを待つ
        await processor.StopAsync();
    }
}


---

2. 実装のポイント

1. Channel<T> を使用

非同期データのキューとして Channel<T> を活用することで、スレッドセーフなデータ処理を実現。



2. 複数のワーカー（Task）で並列処理

コンストラクタで指定した数のワーカーが、非同期にデータを処理。



3. 非同期キューにデータを追加 (EnqueueAsync)

await _channel.Writer.WriteAsync(item); で非同期にデータを追加。



4. データの処理 (ProcessItemsAsync)

ReadAllAsync を使い、非同期でデータを順次処理。



5. 安全な終了 (StopAsync)

_channel.Writer.Complete(); を呼んで書き込みを終了。

_cts.Cancel(); でキャンセル。

Task.WhenAll(_workers); で全ワーカーが終了するのを待つ。





---

3. 応用例

(1) 高速ログプロセッサ

リアルタイムログ処理を非同期で行い、パフォーマンスを向上させる。

var logProcessor = new AsyncDataProcessor<string>(3, async log =>
{
    Console.WriteLine($"[LOG]: {log}");
    await Task.Delay(100); // 仮の遅延
});
await logProcessor.EnqueueAsync("ログメッセージ");


---

(2) 非同期バッチ処理

一定時間ごとにデータをバッチ処理するように応用可能。

var batchProcessor = new AsyncDataProcessor<int>(3, async item =>
{
    Console.WriteLine($"Batch processing: {item}");
    await Task.Delay(200);
});


---

4. まとめ

Channel<T> を使うことで、スレッドセーフかつ非同期なデータ処理が可能

複数ワーカーで並列処理してパフォーマンス向上

安全な終了処理 (StopAsync) を組み込む

ログ処理やバッチ処理に応用できる


このパターンを利用すれば、データの非同期処理をスケーラブルに設計できるので、リアルタイム処理やイベント駆動システムで特に有効です。

