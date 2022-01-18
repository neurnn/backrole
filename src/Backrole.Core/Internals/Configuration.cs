using Backrole.Core.Abstractions;
using System.Collections;
using System.Collections.Generic;

namespace Backrole.Core.Internals
{
    internal class Configuration : IConfiguration
    {
        private Dictionary<string, string> m_KeyValues = new();

        /// <summary>
        /// Initialize a new <see cref="Configuration"/> instance from the Key Value pairs.
        /// </summary>
        /// <param name="KeyValues"></param>
        public Configuration(IEnumerable<(string Key, string Value)> KeyValues)
        {
            foreach (var Each in KeyValues)
                m_KeyValues[Each.Key] = Each.Value;
        }

        /// <inheritdoc/>
        public string this[string Key]
        {
            get
            {
                m_KeyValues.TryGetValue(Key, out var Value);
                return Value;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> Keys => m_KeyValues.Keys;

        /// <inheritdoc/>
        public bool Contains(string Key) => m_KeyValues.ContainsKey(Key);

        /// <inheritdoc/>
        public string Get(string Key, string Default = null)
        {
            m_KeyValues.TryGetValue(Key, out var Value);
            return Value ?? Default;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => m_KeyValues.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => m_KeyValues.GetEnumerator();
    }
}
