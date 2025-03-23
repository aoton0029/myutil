using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Test2
{
    public class ServiceProvider
    {
        private readonly Dictionary<Type, Func<object>> _factories = new();
        private readonly Dictionary<Type, object> _singletons = new();

        public void RegisterSingleton<TService>(TService instance)
        {
            _singletons[typeof(TService)] = instance!;
        }

        public void RegisterSingleton<TService>() where TService : class
        {
            _factories[typeof(TService)] = () => _singletons[typeof(TService)] = CreateInstance(typeof(TService));
        }

        public void RegisterTransient<TService>() where TService : class
        {
            _factories[typeof(TService)] = () => CreateInstance(typeof(TService);
        }

        public TService Resolve<TService>() => (TService)Resolve(typeof(TService));

        public object Resolve(Type type)
        {
            if (_singletons.TryGetValue(type, out var singleton))
            {
                return singleton;
            }

            if (_factories.TryGetValue(type, out var factory))
            {
                return factory();
            }

            // 自動Transientとして作成（未登録でも）
            return CreateInstance(type);
        }

        private object CreateInstance(Type type)
        {
            var constructor = type.GetConstructors()
                                  .OrderByDescending(c => c.GetParameters().Length)
                                  .FirstOrDefault();

            if (constructor == null)
                throw new InvalidOperationException($"No public constructor found for {type.FullName}");

            var parameters = constructor.GetParameters()
                                        .Select(p => Resolve(p.ParameterType))
                                        .ToArray();

            return constructor.Invoke(parameters);
        }
    }
}
