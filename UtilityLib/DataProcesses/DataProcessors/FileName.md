https://maciejz.dev/processing-data-in-parallel-channels/?utm_source=newsletter.csharpdigest.net&utm_medium=referral&utm_campaign=processing-data-in-parallel-using-channels

�f�[�^�� "�O���[�v�� "���邽�߂Ɏg�p�����Key�v���p�e�B���܂ރf�[�^�̒P���ȃ��b�p�[����n�߂܂��傤�B ����Key������2��DataWithKey�C���X�^���X�͕��񏈗��ł��܂���B ��L�̃V�i���I��O���ɒu���ƁAKey�̓f�o�C�X���ʎq���������ƂɂȂ�B IDataProcessor�C���^�[�t�F�[�X�́A�o�b�N�O���E���h�Ńf�[�^���������邽�߂̃X�P�W���[�����O�Ɏg�p�����B

'''
public record DataWithKey(Guid Key, string Data);

public interface IDataProcessor
{
    Task ScheduleDataProcessing(DataWithKey data);
}
'''

�f�[�^�E�v���Z�b�T�̓A�v���P�[�V�����̃��C�t�^�C���S�̂�ʂ��ė��p�\�łȂ���΂Ȃ�Ȃ��̂ŁABackgroundService���N���X���g�p���A�z�X�g���ꂽ�T�[�r�X�Ƃ��ēo�^���邱�Ƃ��ł���B
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
internalQueue�`���l���́AIDataProcessor���[�U�����DataWithKey�C���X�^���X���A�L�[�ŗL�̃v���Z�b�T�ւ̊��蓖�Ă�ҋ@����ꏊ�ɂȂ�B ���̃`���l���̗B��̃��[�_�[��BackgroundDataProcessor�C���X�^���X�ł��邽�߁ASingleReader�I�v�V�����́A�������̍œK���Ɍ����ă����^�C���𓱂����߂Ɏg�p����܂��B ExecuteAsync���\�b�h���X�V���悤�F

'''
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await foreach (var data in _internalQueue.Reader.ReadAllAsync(stoppingToken))
    {
        // TODO: Get or create processor task
        // TODO: Schedule data processing for that processor
    }
}

BackgroundDataProcessor��Singleton�Ȃ̂ŁAconcurrent�o���A���g���g���K�v�͂Ȃ��B �ŏ���TODO�͊ȒP�ł��F

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
�������A2�ڂ͂���������Ԃ�������B �܂��AKeySpecificDataProcessor��p�ӂ��悤�B �L���[�E�`�����l���������A�L���[����A�C�e�������o���A���ۂ̏������s���^�X�N�����B
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
ScheduleData���\�b�h���Ăяo���̂�BackgroundDataProcessor�i�V���O���g���j�����ŁA����Task���B��̃��[�_�[�ł��邱�Ƃ��킩���Ă���̂ŁAKeySpecificDataProcessor._internalQueue��SingleReader��SingleWriter�̗����̃I�v�V������L���ɂ��Ă��܂��B �R���X�g���N�^�̓t�@�N�g���[�E���\�b�h�ɒu���������A�����^�X�N�̊J�n�Ȃ��ɃN���X���쐬���邱�Ƃ͂ł��܂���B ���Ƃ́A�R���X�g���N�^���Ăяo�������CreateAndStartProcessing���\�b�h���g�p����悤��BackgroundDataProcessor���X�V���A�c���TODO���폜���邾���ł��F
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
���ۂ̏����̃}�W�b�N�͕ʂƂ��āA�����ł͂��ꂪ�d�v�Ȃ̂ł͂Ȃ��B �R�[�h�����Ă����v�����낤�F �u�ł��A�Ⴆ��ConcurrentQueue�ɗ��ł����ꂽBlockingCollection�Ɣ�ׂ�ƁA���܂胁���b�g���Ȃ��悤�Ɍ�����B SignleWriter/SingleReader�̍œK���������΁A�܂��債�����_�͂Ȃ��B

���̃V�i���I�ɖ߂�A�������P����邩�����Ă݂悤�B �l������̂́A�f�o�C�X�̌̏��V���b�g�_�E�����B ���̏ꍇ�A���̃f�o�C�X�̃v���Z�b�T�Ɋ֘A���邷�ׂẴ��\�[�X�́A������������Ă��邾���ɂȂ�A���\�[�X��Q��邱�ƂɂȂ�B ���̏ꍇ�A���̃f�o�C�X�̃v���Z�b�T�[�Ɋ֘A���邷�ׂẴ��\�[�X�́A������������Ă��邾���ŁA���\�[�X��Q��邱�ƂɂȂ�B 
�������������ɂ́A3�̂��Ƃ��K�v���F
�eKeySpecificDataProcessors���f�[�^�������I�����������������^�C���X�^���v�A
���݂�KeySpecificDataProcessor�Z�b�g���Ď����A
���΂炭�O�ɏ������I���������̂��N���[���A�b�v���鉽���B
�Ԗڂ̃|�C���g�͈�ԊȒP�Ȃ̂ŁA��������n�߂悤�B 
�����ł́A�`���l���̃N�[���ȋ@�\�̈�ł���A�������}�[�N����A�܂肱��ȏ�f�[�^���������܂Ȃ��Ƃ����@�\���g�����Ƃ��ł���B 
�`���l������ɂȂ�����AChannelReader.ReadAllAsync���\�b�h�͗񋓂��I�����A�����^�X�N�͏I������B ���̃��\�b�h��������s���F
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
�ł́A�^�C���X�^���v�̓_�Ɏ��g��ł݂悤�B ���̓��e�̂��߂ɍŏ��ɊT�O���؂��쐬�����Ƃ��ABackgroundDataProcessor��KeySpecificDataProcessor�Q�Ƃƈꏏ�Ƀ^�C���X�^���v��ۑ����A�������L���[�ɓ��邽�тɍX�V���Ă����B �������A���ɂȂ��āA�����D�ɗ����Ȃ����Ƃ��������B

�����Ɏ��Ԃ�������ꍇ�A�����҂��̃f�[�^���ڂ������L���[�ɓ��邩������Ȃ��B �����f�[�^�����Ȃ��Ȃ�΁A�v���Z�b�T�� "expired "�ƃ}�[�N����邩������Ȃ��B �������A�܂��`���l������̃f�[�^���������Ă��邩������Ȃ��B ���������Ȃ�΁AStopProcessing���\�b�h��await�͊�������܂łɎ��Ԃ�������B

���̂��ߍ���AKeySpecificDataProcessor�͓Ǝ��̃^�C���X�^���v��ێ����邱�ƂɂȂ�B�Ȃ��Ȃ�A�v���Z�b�T�[�������C�ӂ̏u�Ԃ̏�����Ԃ�m���Ă��邩�炾�B
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
Channel.TryPeek���\�b�h�́A�����҂��̃f�[�^�E�A�C�e�����܂����邩�ǂ����������Ă���A��������΁ALastProcessingTimestamp�v���p�e�B�́A�v���Z�b�T���N���[���A�b�v����Ȃ��悤�ɁA���݂̃^�C���X�^���v��Ԃ�������B ���ۂ̃��j�^�����O�̂��߂ɁA�v���Z�b�T�����I�Ƀ`�F�b�N���A�K�v�ł���Β�~���邽�߂ɁA�Ǝ���Task�𐶐�����ʂ̃N���X���쐬���悤�B
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
�������A���ۂɂ���Ă��邱�Ƃ́AKeySpecificDataProcessor��LastProcessingTimestamp���A�w�肳�ꂽ�L�������̂������l�����O�ɂ��邩�ǂ������m�F���邽�߂ɁAPeriodicTimer���g�p����ʂ̃^�X�N�𐶐����邾���ł��B ���������ł���΁AStopProcessing���\�b�h���Ăяo���A�A�N�e�B�u�ȃv���Z�b�T��ێ�����Dictionary����폜����B

����ŁA���g�p���\�[�X�̃N���[���A�b�v�ɕK�v�Ȃ��̂͂��ׂđ������B ���Ƃ�BackgroundDataProcessor�Ŏg���������F
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
���Ă��āA����ł����܂��B���������̖��ʌ����͂�߂悤�B �{���ɒ����L���ɂȂ��Ă��܂����̂ŁA�������Ȃ��������ɂ���̂Ȃ�i���肪�Ƃ��I�j�A�������낻��I���ɂ��悤�Ǝv���Ă��邱�Ƃ��낤�B �����ЂƂ����A�񑩂����B

�����ŁA���ۂ̏������s�����\�b�h�iKeySpecificDataProcessor.ProcessData�j�����Ă݂�ƁA�������Ă��Ȃ����Ƃ��킩�邾�낤�B ����́A�ˑ��֌W���Ȃ��Ɖ����ł��Ȃ����炾�B �K�v�Ȃ��̂����ׂĐV��������΂����̂����A����͂�߂����������B

���̂悤�Ȉˑ��֌W��z�����Ă݂Ăق����F
'''
public interface IDependency
{
    Task DoStuff();
}
'''
KeySpecificDataProcessor���ADI�R���e�i����������郁�J�j�Y���Ŋg�����Ă݂悤�B
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
�����ł́A�V�����f�[�^���ڂ���������邽�тɐV�����X�R�[�v���쐬���Ă��邪�A���ꂪ�ł����R�ȃX�R�[�v���Ɗ����邩�炾�B�������A�j�[�Y�ɍ����΁A�L�[���Ƃ̃f�[�^�E�v���Z�b�T�ɕύX���邱�Ƃ��ȒP���B

���ɁABackgroundDataProcessor���X�V���āA�R���X�g���N�^�����ɂ����IServiceScopeFactory���擾���AKeySpecificDataProcessor�C���X�^���X�ɓn���܂��B
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

�������A�v���Z�b�T�[�����P���邽�߂ɂł��邱�Ƃ͂����Ƃ���B �G���[�����A�L���[�̃f�[�^�ێ��A�e�X�g�A����ɂ��ߍׂ��ȃ��b�N�@�\��ǉ����āA�{���ɒe�͓I�ŐM���ł��鏈���ɂ���K�v������B �܂��A�����̃j�[�Y�ɍœK�ɂȂ�悤�ɒ�������̂͌����܂ł��Ȃ��B����LongRunning�I�v�V������Tasks���g�����Ƃɂ������AThreads�̕���������������Ȃ����A�Z����Tasks�̕���������������Ȃ��B �Ď��Ԋu���o���I�ɒ�������K�v������B �������A����͂��Ȃ��̃V�X�e���ɂƂ��Ĕ��ɕ��G�ȕ����ɂȂ��Ă��܂���������Ȃ��B �����̂悤�ɁATPL Dataflow���C�u�����̂悤�ȑ�ֈĂ��������A�������₷���ێ炵�₷���Ƃ����_�ŁA���Ȃ��̃V�X�e���ƃ`�[���ɂƂ��ĉ����x�X�g�������߂�ׂ����B