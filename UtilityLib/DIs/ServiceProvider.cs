using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DIs
{

    /// <summary>
    /// Options for configuring various behaviors of the default <see cref="IServiceProvider"/> implementation.
    /// </summary>
    public class ServiceProviderOptions
    {
        // Avoid allocating objects in the default case
        internal static readonly ServiceProviderOptions Default = new ServiceProviderOptions();

        /// <summary>
        /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>.
        /// </summary>
        public bool ValidateScopes { get; set; }
    }

    /// <summary>
    /// The default IServiceProvider.
    /// </summary>
    public sealed class ServiceProvider : IServiceProvider, IDisposable
    {
        private readonly Dictionary<Type, ServiceDescriptor> _serviceDescriptors;

        public ServiceProvider(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            _serviceDescriptors = serviceDescriptors.ToDictionary(d => d.ServiceType);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            if (!_serviceDescriptors.TryGetValue(serviceType, out var descriptor))
            {
                throw new Exception($"Service of type {serviceType.Name} is not registered.");
            }

            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            var implementation = Activator.CreateInstance(descriptor.ImplementationType);

            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                descriptor.SetInstance(implementation);
            }

            return implementation;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }
    }
}
