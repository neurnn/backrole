using System;
using System.Collections.Generic;

namespace Backrole.Core.Internals.Services
{
    /// <summary>
    /// Provides the hardcoded services.
    /// </summary>
    internal class ServiceHardcordedProvider : IServiceProvider
    {
        private Dictionary<Type, object> m_Instances = new();

        /// <summary>
        /// Set the hardcoded service to <see cref="ServiceHardcordedProvider"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Instance"></param>
        /// <returns></returns>
        public ServiceHardcordedProvider Set(Type ServiceType, object Instance)
        {
            lock (this)
            {
                if (Instance is null)
                {
                    m_Instances.Remove(ServiceType);
                    return this;
                }

                m_Instances[ServiceType] = Instance;
                return this;
            }
        }

        /// <summary>
        /// Set the hardcoded service to <see cref="ServiceHardcordedProvider"/>.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="Instance"></param>
        /// <returns></returns>
        public ServiceHardcordedProvider Set<ServiceType>(object Instance)
            => Set(typeof(ServiceType), Instance);

        /// <inheritdoc/>
        public object GetService(Type ServiceType)
        {
            if (m_Instances.TryGetValue(ServiceType, out var Instance))
                return Instance;

            return null;
        }
    }
}
