using Backrole.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace Backrole.Core.Internals.Services
{
    internal class ServiceCollectionView
    {
        private Dictionary<Type, IServiceRegistration> m_Registrations = new();
        private ServiceIntegratedExtension m_Extension;

        /// <summary>
        /// Initialize a new <see cref="ServiceCollectionView"/> from the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="Services"></param>
        public ServiceCollectionView(IServiceCollection Services)
        {
            foreach(var Each in Services)
                m_Registrations[Each.Type] = Each;

            m_Extension = new ServiceIntegratedExtension(Services);
        }

        /// <summary>
        /// Alter the registration informations.
        /// </summary>
        /// <param name="Alter"></param>
        /// <returns></returns>
        public ServiceCollectionView Alter(Action<IDictionary<Type, IServiceRegistration>> Alter)
        {
            Alter?.Invoke(m_Registrations);
            return this;
        }

        /// <summary>
        /// Gets the integrated extension.
        /// </summary>
        public IServiceExtension Extension => m_Extension;

        /// <summary>
        /// Gets the <see cref="IServiceRegistration"/> by its type.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public IServiceRegistration Find(Type ServiceType)
        {
            if (m_Registrations.TryGetValue(ServiceType, out var Registration))
                return Registration;

            if (ServiceType.IsConstructedGenericType)
            {
                var Generic = ServiceType.GetGenericTypeDefinition();
                if (m_Registrations.TryGetValue(Generic, out Registration))
                    return Registration;
            }

            return null;
        }
    }
}
