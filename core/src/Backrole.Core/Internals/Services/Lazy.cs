using Backrole.Core.Abstractions;
using System;

namespace Backrole.Core.Internals.Services
{
    internal class Lazy<ServiceType> : ILazy<ServiceType>
    {
        private IServiceProvider m_Services;
        private ServiceType m_Instance;
        private bool m_Resolved = false;

        /// <summary>
        /// Initialize a new <see cref="Lazy{ServiceType}"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        public Lazy(IServiceProvider Services)
        {
            m_Services = Services;
        }

        /// <inheritdoc/>
        public ServiceType Instance
        {
            get
            {
                lock(this)
                {
                    if (!m_Resolved)
                    {
                        m_Resolved = true;
                        m_Instance = (ServiceType) m_Services.GetService(typeof(ServiceType));
                    }

                    return m_Instance;
                }
            }
        }
    }
}
