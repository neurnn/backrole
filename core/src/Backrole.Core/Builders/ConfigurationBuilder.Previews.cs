using Backrole.Core.Abstractions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Backrole.Core.Builders
{
    public partial class ConfigurationBuilder
    {
        /// <summary>
        /// Previes the <see cref="ConfigurationBuilder"/>'s contents as <see cref="IConfiguration"/>.
        /// </summary>
        private class Previews : IConfiguration
        {
            private ConfigurationBuilder m_Builder;

            /// <summary>
            /// Initialize a new <see cref="Previews"/> instance.
            /// </summary>
            /// <param name="Builder"></param>
            public Previews(ConfigurationBuilder Builder) => m_Builder = Builder;

            /// <inheritdoc/>
            public string this[string Key] => m_Builder.Get(Key);

            /// <inheritdoc/>
            public IEnumerable<string> Keys => m_Builder.GetKeys();

            /// <inheritdoc/>
            public bool Contains(string Key) => m_Builder.Get(Key) != null;

            /// <inheritdoc/>
            public string Get(string Key, string Default = null) => m_Builder.Get(Key, Default);

            /// <inheritdoc/>
            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                var Temp = m_Builder.GetKeys()
                    .Select(X => new KeyValuePair<string, string>(X, Get(X)))
                    .Where(X => X.Value != null);

                return Temp.GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
