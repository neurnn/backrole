using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Backrole.Core.Abstractions.Defaults
{
    /// <summary>
    /// Basic implementation of <see cref="IServiceProperties"/> interface.
    /// </summary>
    public class ServiceProperties : IServiceProperties
    {
        private Dictionary<object, object> m_KeyValues = new();

        /// <inheritdoc/>
        public object this[object key]
        {
            get => m_KeyValues[key]; 
            set => m_KeyValues[key] = value;
        }

        /// <inheritdoc/>
        public ICollection<object> Keys => m_KeyValues.Keys;

        /// <inheritdoc/>
        public ICollection<object> Values => m_KeyValues.Values;

        /// <inheritdoc/>
        public int Count => m_KeyValues.Count;

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<object, object>>.IsReadOnly => false;

        /// <inheritdoc/>
        public void Add(object key, object value) => m_KeyValues.Add(key, value);

        /// <inheritdoc/>
        void ICollection<KeyValuePair<object, object>>.Add(KeyValuePair<object, object> item) => Add(item.Key, item.Value);

        /// <inheritdoc/>
        public void Clear() => m_KeyValues.Clear();

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<object, object> item) => ContainsKey(item.Key);

        /// <inheritdoc/>
        public bool ContainsKey(object key) => m_KeyValues.ContainsKey(key);

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            if (m_KeyValues is ICollection<KeyValuePair<object, object>> Collection)
                Collection.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() => m_KeyValues.GetEnumerator();

        /// <inheritdoc/>
        public bool Remove(object key)
        {
            return m_KeyValues.Remove(key);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<object, object> item) => Remove(item.Key);

        /// <inheritdoc/>
        public bool TryGetValue(object key, [MaybeNullWhen(false)] out object value)
            => m_KeyValues.TryGetValue(key, out value);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
