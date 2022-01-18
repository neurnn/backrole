using System;
using System.Collections;
using System.Collections.Generic;

namespace Backrole.Core.Abstractions.Defaults
{
    public class ServiceExtensionCollection : IServiceExtensionCollection
    {
        private List<IServiceExtension> m_Extensions = new();

        /// <inheritdoc/>
        public int Count => m_Extensions.Count;

        /// <inheritdoc/>
        bool ICollection<IServiceExtension>.IsReadOnly => false;

        /// <inheritdoc/>
        public void Clear() => m_Extensions.Clear();

        /// <inheritdoc/>
        public void Add(IServiceExtension item) => m_Extensions.Add(item);

        /// <inheritdoc/>
        public bool Contains(IServiceExtension item) => m_Extensions.Contains(item);

        /// <inheritdoc/>
        void ICollection<IServiceExtension>.CopyTo(IServiceExtension[] array, int arrayIndex) => m_Extensions.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public IServiceExtension Find(Type ExtensionType) => m_Extensions.Find(X => X.GetType() == ExtensionType);

        /// <inheritdoc/>
        public IServiceExtension FindLast(Type ExtensionType) => m_Extensions.FindLast(X => X.GetType() == ExtensionType);

        /// <inheritdoc/>
        public IEnumerator<IServiceExtension> GetEnumerator() => m_Extensions.GetEnumerator();

        /// <inheritdoc/>
        public bool Remove(IServiceExtension item) => m_Extensions.Remove(item);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
