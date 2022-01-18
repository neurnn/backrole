using Backrole.Core.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace Backrole.Core.Internals.Services
{
    internal class ServiceIntegratedExtension : IServiceExtension
    {
        private IServiceExtension[] m_Extensions;

        /// <summary>
        /// Initialize a new <see cref="ServiceIntegratedExtension"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        public ServiceIntegratedExtension(IServiceCollection Services) 
            => m_Extensions = Services.Extensions.Distinct().ToArray();

        /// <inheritdoc/>
        public object GetService(IServiceProvider ScopedServices, Type RequestedType)
        {
            foreach(var Each in m_Extensions)
            {
                var Service = Each.GetService(ScopedServices, RequestedType);
                if (Service is null)
                    continue;

                return Service;
            }

            return null;
        }

        /// <inheritdoc/>
        public object GetService(IServiceProvider ScopedServices, Type TargetType, ICustomAttributeProvider Attributes)
        {
            foreach (var Each in m_Extensions)
            {
                var Service = Each.GetService(ScopedServices, TargetType, Attributes);
                if (Service is null)
                    continue;

                return Service;
            }

            return null;
        }
    }
}
