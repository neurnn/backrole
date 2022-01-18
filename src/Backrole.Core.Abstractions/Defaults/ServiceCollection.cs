using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Backrole.Core.Abstractions.Defaults
{
    public class ServiceCollection : IServiceCollection
    {
        private List<IServiceRegistration> m_Services = new();
        private List<Action<IServiceProvider>> m_Configures = new();

        /// <summary>
        /// Initialize a new <see cref="ServiceCollection"/> instance.
        /// </summary>
        /// <param name="Properties"></param>
        public ServiceCollection(IServiceProperties Properties = null)
            => this.Properties = Properties ?? new ServiceProperties();

        /// <inheritdoc/>
        public IServiceProperties Properties { get; } 

        /// <inheritdoc/>
        public IServiceExtensionCollection Extensions { get; } = new ServiceExtensionCollection();

        /// <inheritdoc/>
        public Action<IServiceProvider> ConfigureDelegate
        {
            get
            {
                if (m_Configures.Count <= 1)
                    return m_Configures.FirstOrDefault();

                var Queue = new Queue<Action<IServiceProvider>>(m_Configures);
                return (Services) =>
                {
                    while (Queue.TryDequeue(out var Configure))
                        Configure?.Invoke(Services);
                };
            }
        }

        /// <inheritdoc/>
        public int Count => m_Services.Count;

        /// <inheritdoc/>
        bool ICollection<IServiceRegistration>.IsReadOnly => false;

        /// <inheritdoc/>
        public void Clear() => m_Services.Clear();

        /// <inheritdoc/>
        public void Add(IServiceRegistration item) => m_Services.Add(item);

        /// <inheritdoc/>
        public bool Contains(IServiceRegistration item) => m_Services.Contains(item);

        /// <inheritdoc/>
        void ICollection<IServiceRegistration>.CopyTo(IServiceRegistration[] array, int arrayIndex) => m_Services.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public IServiceRegistration Find(Type ServiceType) => m_Services.Find(X => X.Type == ServiceType);

        /// <inheritdoc/>
        public IServiceRegistration FindLast(Type ServiceType) => m_Services.FindLast(X => X.Type == ServiceType);

        /// <inheritdoc/>
        public IEnumerator<IServiceRegistration> GetEnumerator() => m_Services.GetEnumerator();

        /// <inheritdoc/>
        public bool Remove(IServiceRegistration item) => m_Services.Remove(item);

        /// <inheritdoc/>
        public IServiceCollection Configure(Action<IServiceProvider> Delegate)
        {
            m_Configures.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
