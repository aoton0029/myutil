
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DIs.Simple
{
    // ======== DIコンテナの基本クラス =========

    public class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public object ImplementationInstance { get; }

        public ServiceDescriptor(Type serviceType, Type implementationType)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        public ServiceDescriptor(Type serviceType, object implementationInstance)
        {
            ServiceType = serviceType;
            ImplementationInstance = implementationInstance;
        }
    }

    public class ServiceCollection
    {
        private readonly List<ServiceDescriptor> _services = new();

        public void AddSingleton<TService, TImplementation>()
        {
            _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation)));
        }

        public void AddSingleton<TService>(TService implementationInstance)
        {
            _services.Add(new ServiceDescriptor(typeof(TService), implementationInstance));
        }

        public ServiceProvider BuildServiceProvider() => new(_services);
    }

    public class ServiceProvider
    {
        private readonly Dictionary<Type, object> _singletonInstances = new();
        private readonly List<ServiceDescriptor> _serviceDescriptors;

        public ServiceProvider(List<ServiceDescriptor> serviceDescriptors)
        {
            _serviceDescriptors = serviceDescriptors;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        private object GetService(Type serviceType)
        {
            var descriptor = _serviceDescriptors.FirstOrDefault(s => s.ServiceType == serviceType);
            if (descriptor == null)
                throw new InvalidOperationException($"サービス {serviceType.Name} は登録されていません");

            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (_singletonInstances.TryGetValue(serviceType, out var existingInstance))
                return existingInstance;

            var implementation = Activator.CreateInstance(descriptor.ImplementationType);
            _singletonInstances[serviceType] = implementation;
            return implementation;
        }
    }

    // ======== ホストとホスティングの管理 =========

    public interface IHost
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }

    public class Host : IHost
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly List<IHostedService> _hostedServices = new();

        public Host(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _hostedServices = _serviceProvider.GetService<IEnumerable<IHostedService>>()?.ToList() ?? new List<IHostedService>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Host starting...");
            foreach (var service in _hostedServices)
            {
                await service.StartAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Host stopping...");
            foreach (var service in _hostedServices)
            {
                await service.StopAsync(cancellationToken);
            }
        }
    }

    public interface IHostBuilder
    {
        IHostBuilder ConfigureServices(Action<ServiceCollection> configure);
        IHost Build();
    }

    public class HostBuilder : IHostBuilder
    {
        private readonly ServiceCollection _services = new();

        public IHostBuilder ConfigureServices(Action<ServiceCollection> configure)
        {
            configure(_services);
            return this;
        }

        public IHost Build()
        {
            var serviceProvider = _services.BuildServiceProvider();
            return new Host(serviceProvider);
        }
    }

    // ======== バックグラウンドサービスの管理 =========

    public interface IHostedService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }

    public abstract class BackgroundService : IHostedService
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_stoppingCts.Token);
            await _executingTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingCts.Cancel();
            await (_executingTask ?? Task.CompletedTask);
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }

    public class SampleBackgroundService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("バックグラウンドサービス実行中...");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    // ======== 実行コード =========
    //class Program
    //{
    //    static async Task Main(string[] args)
    //    {
    //        var host = new HostBuilder()
    //            .ConfigureServices(services =>
    //            {
    //                services.AddSingleton<IHostedService, SampleBackgroundService>();
    //            })
    //            .Build();

    //        var cts = new CancellationTokenSource();

    //        Console.CancelKeyPress += (sender, eventArgs) =>
    //        {
    //            eventArgs.Cancel = true;
    //            cts.Cancel();
    //        };

    //        await host.StartAsync(cts.Token);
    //        await host.StopAsync(cts.Token);
    //    }
    //}

    // ======== データベースサービス =========

    public interface IDatabaseService
    {
        List<string> FetchData();
    }

    public class DatabaseService : IDatabaseService
    {
        public List<string> FetchData()
        {
            // 実際のデータベース接続に置き換え（Dapper や EF Core など）
            return new List<string> { "データ1", "データ2", "データ3" };
        }
    }

    // ======== データベースを定期的に取得するサービス =========

    public class DatabaseFetcherService : BackgroundService
    {
        private readonly IDatabaseService _databaseService;

        public DatabaseFetcherService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("データを取得中...");
                var data = _databaseService.FetchData();

                Console.WriteLine($"取得したデータ: {string.Join(", ", data)}");

                await Task.Delay(5000, stoppingToken); // 5秒ごとに取得
            }
        }
    }

    // ======== 実行コード =========

    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IDatabaseService, DatabaseService>(); // DBサービス登録
                    services.AddSingleton<IHostedService, DatabaseFetcherService>(); // 定期取得サービス登録
                })
                .Build();

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            await host.StartAsync(cts.Token);
            await host.StopAsync(cts.Token);
        }
    }
}
