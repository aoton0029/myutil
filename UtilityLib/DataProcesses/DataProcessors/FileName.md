https://maciejz.dev/processing-data-in-parallel-channels/?utm_source=newsletter.csharpdigest.net&utm_medium=referral&utm_campaign=processing-data-in-parallel-using-channels

データを "グループ化 "するために使用されるKeyプロパティを含むデータの単純なラッパーから始めましょう。 同じKeyを持つ2つのDataWithKeyインスタンスは並列処理できません。 上記のシナリオを念頭に置くと、Keyはデバイス識別子を示すことになる。 IDataProcessorインターフェースは、バックグラウンドでデータを処理するためのスケジューリングに使用される。

'''
public record DataWithKey(Guid Key, string Data);

public interface IDataProcessor
{
    Task ScheduleDataProcessing(DataWithKey data);
}
'''

データ・プロセッサはアプリケーションのライフタイム全体を通して利用可能でなければならないので、BackgroundService基底クラスを使用し、ホストされたサービスとして登録することができる。
'''
public class BackgroundDataProcessor : BackgroundService, IDataProcessor
{
    private readonly Channel<DataWithKey> _internalQueue = Channel.CreateUnbounded<Data>(new UnboundedChannelOptions { SingleReader = true });

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: Process data from _internalQueue
        return Task.CompletedTask;
    }

    public async Task ScheduleDataProcessing(DataWithKey data) => await _internalQueue.Writer.WriteAsync(data);
}
'''
internalQueueチャネルは、IDataProcessorユーザからのDataWithKeyインスタンスが、キー固有のプロセッサへの割り当てを待機する場所になる。 そのチャネルの唯一のリーダーはBackgroundDataProcessorインスタンスであるため、SingleReaderオプションは、いくつかの最適化に向けてランタイムを導くために使用されます。 ExecuteAsyncメソッドを更新しよう：

'''
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await foreach (var data in _internalQueue.Reader.ReadAllAsync(stoppingToken))
    {
        // TODO: Get or create processor task
        // TODO: Schedule data processing for that processor
    }
}

BackgroundDataProcessorはSingletonなので、concurrentバリアントを使う必要はない。 最初のTODOは簡単です：

public class BackgroundDataProcessor : BackgroundService, IDataProcessor
{
    ...
    private readonly Dictionary<Guid, KeySpecificDataProcessor> _dataProcessors = new();
    ...
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var data in _internalQueue.Reader.ReadAllAsync(stoppingToken))
        {
            var processor = GetOrCreateDataProcessor(data.Key);
            // TODO: Schedule data processing for that processor
        }
    }

    private KeySpecificDataProcessor GetOrCreateDataProcessor(Guid key)
    {
        if (!_dataProcessors.ContainsKey(key))
        {
            _dataProcessors[key] = new KeySpecificDataProcessor(key);
        }

        return _dataProcessors[key];
    }
    ...
}

public class KeySpecificDataProcessor
{
    public Guid ProcessorKey { get; }

    public KeySpecificDataProcessor(Guid processorKey)
    {
        ProcessorKey = processorKey;
    }
}
'''
しかし、2つ目はもう少し手間がかかる。 まず、KeySpecificDataProcessorを用意しよう。 キュー・チャンネルも持つが、キューからアイテムを取り出し、実際の処理を行うタスクも持つ。
'''
public class KeySpecificDataProcessor : IDataProcessor
{
    public Guid ProcessorKey { get; }
    
    private Task? _processingTask;
    
    private readonly Channel<DataWithKey> _internalQueue = Channel.CreateUnbounded<Data>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });

    private KeySpecificDataProcessor(Guid processorKey)
    {
        ProcessorKey = processorKey;
    }

    private void StartProcessing(CancellationToken cancellationToken = default)
    {
        _processingTask = Task.Factory.StartNew(
            async () =>
            {
                await foreach (var data in _internalQueue.Reader.ReadAllAsync(cancellationToken))
                {
                    // TODO: Process data
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public async Task ScheduleDataProcessing(DataWithKey data)
    {
        if (data.Key != ProcessorKey)
        {
            throw new InvalidOperationException($"Data with key {data.Key} scheduled for KeySpecificDataProcessor with key {ProcessorKey}");
        }
        
        await _internalQueue.Writer.WriteAsync(data);
    }

    public static KeySpecificDataProcessor CreateAndStartProcessing(Guid processorKey, CancellationToken processingCancellationToken = default)
    {
        var instance = new KeySpecificDataProcessor(processorKey);
        instance.StartProcessing(processingCancellationToken);
        return instance;
    }
}
'''
ScheduleDataメソッドを呼び出すのはBackgroundDataProcessor（シングルトン）だけで、内部Taskが唯一のリーダーであることがわかっているので、KeySpecificDataProcessor._internalQueueはSingleReaderとSingleWriterの両方のオプションを有効にしています。 コンストラクタはファクトリー・メソッドに置き換えられ、処理タスクの開始なしにクラスを作成することはできません。 あとは、コンストラクタを呼び出す代わりにCreateAndStartProcessingメソッドを使用するようにBackgroundDataProcessorを更新し、残りのTODOを削除するだけです：
'''
public class BackgroundDataProcessor : BackgroundService, IDataProcessor
{
    ...
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var data in _internalQueue.Reader.ReadAllAsync(stoppingToken))
        {
            var processor = GetOrCreateDataProcessor(data.Key);
            processor.ScheduleDataProcessing(data)
        }
    }

    private KeySpecificDataProcessor GetOrCreateDataProcessor(Guid key, CancellationToken processorCancellationToken = default)
    {
        if (!_dataProcessors.ContainsKey(key))
        {
            _dataProcessors[key] = KeySpecificDataProcessor.CreateAndStartProcessing(key, processorCancellationToken);
        }

        return _dataProcessors[key];
    }
    ...
}
'''
実際の処理のマジックは別として、ここではそれが重要なのではない。 コードを見てこう思うだろう： 「でも、例えばConcurrentQueueに裏打ちされたBlockingCollectionと比べると、あまりメリットがないように見える。 SignleWriter/SingleReaderの最適化を除けば、まだ大した利点はない。

元のシナリオに戻り、何が改善されるかを見てみよう。 考えられるのは、デバイスの故障やシャットダウンだ。 その場合、そのデバイスのプロセッサに関連するすべてのリソースは、ただ横たわっているだけになり、リソースを浪費することになる。 その場合、そのデバイスのプロセッサーに関連するすべてのリソースは、ただ横たわっているだけで、リソースを浪費することになる。 
これを解決するには、3つのことが必要だ：
各KeySpecificDataProcessorsがデータ処理を終了した時刻を示すタイムスタンプ、
現在のKeySpecificDataProcessorセットを監視し、
しばらく前に処理を終了したものをクリーンアップする何か。
番目のポイントは一番簡単なので、ここから始めよう。 
ここでは、チャネルのクールな機能の一つである、完了をマークする、つまりこれ以上データを書き込まないという機能を使うことができる。 
チャネルが空になったら、ChannelReader.ReadAllAsyncメソッドは列挙を終了し、処理タスクは終了する。 次のメソッドがそれを行う：
'''
public class KeySpecificDataProcessor : IDataProcessor
{
    ...
    public async Task StopProcessing()
    {
        _internalQueue.Writer.Complete();
        if (_processingTask != null)
        {
            await _processingTask;
        }
    }
    ...
}
'''
では、タイムスタンプの点に取り組んでみよう。 この投稿のために最初に概念実証を作成したとき、BackgroundDataProcessorのKeySpecificDataProcessor参照と一緒にタイムスタンプを保存し、何かがキューに入るたびに更新していた。 しかし、今になって、何か腑に落ちないことがあった。

処理に時間がかかる場合、処理待ちのデータ項目が長いキューに入るかもしれない。 もしデータが来なくなれば、プロセッサは "expired "とマークされるかもしれない。 しかし、まだチャネルからのデータを処理しているかもしれない。 もしそうなれば、StopProcessingメソッドのawaitは完了するまでに時間がかかる。

そのため今回、KeySpecificDataProcessorは独自のタイムスタンプを保持することになる。なぜなら、プロセッサーだけが任意の瞬間の処理状態を知っているからだ。
'''
public class KeySpecificDataProcessor : IDataProcessor
{
    ...
    public DateTime LastProcessingTimestamp => _processingFinishedTimestamp ?? DateTime.UtcNow;

    private DateTime? _processingFinishedTimestamp = DateTime.UtcNow;

    private bool Processing
    {
        set
        {
            if (!value)
            {
                _processingFinishedTimestamp = DateTime.UtcNow;
            }
            else
            {
                _processingFinishedTimestamp = null;
            }
        }
    }
    ...
    private void StartProcessing(CancellationToken cancellationToken = default)
    {
        _processingTask = Task.Factory.StartNew(
            async () =>
            {
                await foreach (var data in _internalQueue.Reader.ReadAllAsync(cancellationToken))
                {
                    Processing = true;
                    await ProcessData(data);
                    Processing = _internalQueue.Reader.TryPeek(out _);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task ProcessData(DataWithKey data)
    {
        // TODO: Process data
        await _dependency.DoStuff();
    }

    public async Task ScheduleDataProcessing(DataWithKey dataWithKey)
    {
        if (dataWithKey.Key != ProcessorKey)
        {
            throw new InvalidOperationException($"Data with key {dataWithKey.Key} scheduled for KeySpecificDataProcessor with key {ProcessorKey}");
        }

        Processing = true;
        await _internalQueue.Writer.WriteAsync(dataWithKey);
    }
    ...
}
'''
Channel.TryPeekメソッドは、処理待ちのデータ・アイテムがまだあるかどうかを教えてくれ、もしあれば、LastProcessingTimestampプロパティは、プロセッサがクリーンアップされないように、現在のタイムスタンプを返し続ける。 実際のモニタリングのために、プロセッサを定期的にチェックし、必要であれば停止するために、独自のTaskを生成する別のクラスを作成しよう。
'''
public partial class BackgroundDataProcessor
{
    public class BackgroundDataProcessorMonitor
    {
        private readonly TimeSpan _processorExpiryThreshold = TimeSpan.FromSeconds(30);
    
        private readonly TimeSpan _processorExpiryScanningPeriod = TimeSpan.FromSeconds(5);

        private MonitoringTask? _monitoringTask;

        private readonly SemaphoreSlim _processorsLock;

        private readonly Dictionary<Guid, KeySpecificDataProcessor> _dataProcessors;

        private BackgroundDataProcessorMonitor(SemaphoreSlim processorsLock, Dictionary<Guid, KeySpecificDataProcessor> dataProcessors)
        {
            _processorsLock = processorsLock;
            _dataProcessors = dataProcessors;
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
                        await expiredProcessor..StopProcessing();
                        _dataProcessors.Remove(expiredProcessor.ProcessorKey);
                    }
                    
                    _processorsLock.Release();
                }
            }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _monitoringTask = new MonitoringTask(task, tokenSource);
        }

        private bool IsExpired(KeySpecificDataProcessor processor) => (DateTime.UtcNow - processor.LastProcessingTimestamp) > _processorExpiryThreshold;

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

        public static BackgroundDataProcessorMonitor CreateAndStartMonitoring(SemaphoreSlim processorsLock, Dictionary<Guid, KeySpecificDataProcessor> dataProcessors, CancellationToken monitoringCancellationToken = default)
        {
            var monitor = new BackgroundDataProcessorMonitor(processorsLock, dataProcessors);
            monitor.StartMonitoring(monitoringCancellationToken);
            return monitor;
        }

        private readonly record struct MonitoringTask(Task Task, CancellationTokenSource CancellationTokenSource);
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
'''
しかし、実際にやっていることは、KeySpecificDataProcessorのLastProcessingTimestampが、指定された有効期限のしきい値よりも前にあるかどうかを確認するために、PeriodicTimerを使用する別のタスクを生成するだけです。 もしそうであれば、StopProcessingメソッドを呼び出し、アクティブなプロセッサを保持するDictionaryから削除する。

これで、未使用リソースのクリーンアップに必要なものはすべて揃った。 あとはBackgroundDataProcessorで使うだけだ：
'''
public partial class BackgroundDataProcessor : BackgroundService, IDataProcessor
{
    ...
    private readonly SemaphoreSlim _processorsLock = new(1, 1);

    private BackgroundDataProcessorMonitor? _monitor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _monitor = BackgroundDataProcessorMonitor.CreateAndStartMonitoring(_processorsLock, _dataProcessors, _loggerFactory.CreateLogger<BackgroundDataProcessorMonitor>(), stoppingToken);
        await foreach (var data in _internalQueue.Reader.ReadAllAsync(stoppingToken))
        {
            if (!await _processorsLock.WaitWithCancellation(stoppingToken))
            {
                break;
            }
            
            var processor = GetOrCreateDataProcessor(data.Key, stoppingToken);
            await processor.ScheduleDataProcessing(data);

            _processorsLock.Release();
        }

        await _monitor.StopMonitoring();
    }
    
    private KeySpecificDataProcessor GetOrCreateDataProcessor(Guid key, CancellationToken newProcessorCancellationToken = default)
    {
        if (!_dataProcessors.TryGetValue(key, out var deviceProcessor))
        {
            var processor = CreateNewProcessor(key, newProcessorCancellationToken);
            _dataProcessors[key] = processor;
            deviceProcessor = processor;
        }
        
        return deviceProcessor.Processor;
    }
    ...
}
'''
さてさて、これでおしまい。もう資源の無駄遣いはやめよう。 本当に長い記事になってしまったので、もしあなたがここにいるのなら（ありがとう！）、もうそろそろ終わりにしようと思っていることだろう。 もうひとつだけ、約束するよ。

そこで、実際の処理を行うメソッド（KeySpecificDataProcessor.ProcessData）を見てみると、何もしていないことがわかるだろう。 これは、依存関係がないと何もできないからだ。 必要なものをすべて新しくすればいいのだが、それはやめた方がいい。

次のような依存関係を想像してみてほしい：
'''
public interface IDependency
{
    Task DoStuff();
}
'''
KeySpecificDataProcessorを、DIコンテナから解決するメカニズムで拡張してみよう。
'''
public class KeySpecificDataProcessor : IDataProcessor
{
    ...
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private KeySpecificDataProcessor(Guid processorKey, IServiceScopeFactory serviceScopeFactory)
    {
        ProcessorKey = processorKey;
        _serviceScopeFactory = serviceScopeFactory;
    }

    private void StartProcessing(CancellationToken cancellationToken = default)
    {
        _processingTask = Task.Factory.StartNew(
            async () =>
            {
                await foreach (var data in _internalQueue.Reader.ReadAllAsync(cancellationToken))
                {
                    Processing = true;
                    using (var dependenciesProvider = new DependenciesProvider(_serviceScopeFactory))
                    {
                        await ProcessData(data, dependenciesProvider.Dependency);
                    }
                    
                    Processing = _internalQueue.Reader.TryPeek(out _);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
    
    private async Task ProcessData(DataWithKey data, IDependency dependency)
    {
        await dependency.DoStuff();
    }
    ...
    public static KeySpecificDataProcessor CreateAndStartProcessing(Guid processorKey, IServiceScopeFactory serviceScopeFactory, CancellationToken processingCancellationToken = default)
    {
        var instance = new KeySpecificDataProcessor(processorKey, serviceScopeFactory);
        instance.StartProcessing(processingCancellationToken);
        return instance;
    }

    private class DependenciesProvider : IDisposable
    {
        private readonly IServiceScope _scope;

        public IDependency Dependency { get; }

        public DependenciesProvider(IServiceScopeFactory serviceScopeFactory)
        {
            _scope = serviceScopeFactory.CreateScope();
            Dependency = _scope.ServiceProvider.GetRequiredService<IDependency>();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
'''
ここでは、新しいデータ項目が処理されるたびに新しいスコープを作成しているが、それが最も自然なスコープだと感じるからだ。もちろん、ニーズに合えば、キーごとのデータ・プロセッサに変更することも簡単だ。

次に、BackgroundDataProcessorを更新して、コンストラクタ注入によってIServiceScopeFactoryを取得し、KeySpecificDataProcessorインスタンスに渡します。
'''
public partial class BackgroundDataProcessor : BackgroundService, IDataProcessor
{
    ...
    private readonly IServiceScopeFactory _serviceScopeFactory;
    ...
    public BackgroundDataProcessor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    ...
    private KeySpecificDataProcessor CreateNewProcessor(int dataKey, CancellationToken processorCancellationToken = default)
    {
        return KeySpecificDataProcessor.CreateAndStartProcessing(dataKey, _serviceScopeFactory, processorCancellationToken);
    }
    ...
}
'''

もちろん、プロセッサーを改善するためにできることはもっとある。 エラー処理、キューのデータ保持、テスト、さらにきめ細かなロック機構を追加して、本当に弾力的で信頼できる処理にする必要がある。 また、自分のニーズに最適になるように調整するのは言うまでもない。私はLongRunningオプションでTasksを使うことにしたが、Threadsの方がいいかもしれないし、短命のTasksの方がいいかもしれない。 監視間隔も経験的に調整する必要がある。 もちろん、これはあなたのシステムにとって非常に複雑な部分になってしまうかもしれない。 いつものように、TPL Dataflowライブラリのような代替案を検討し、理解しやすく保守しやすいという点で、あなたのシステムとチームにとって何がベストかを決めるべきだ。