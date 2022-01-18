using Backrole.Core.Abstractions;
using System.Collections;
using System.Collections.Generic;

namespace Backrole.Core.Internals.Fallbacks
{
    internal class NullConfiguration : IConfiguration
    {
        private static readonly List<KeyValuePair<string, string>> EMPTY_KV = new();
        private static readonly string[] EMPTY = new string[0];

        /// <inheritdoc/>
        public string this[string Key] => null;

        /// <inheritdoc/>
        public IEnumerable<string> Keys => EMPTY;

        /// <inheritdoc/>
        public bool Contains(string Key) => false;

        /// <inheritdoc/>
        public string Get(string Key, string Default = null) => Default;

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => EMPTY_KV.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => EMPTY_KV.GetEnumerator();
    }
}
