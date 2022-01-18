using Backrole.Core.Abstractions;
using Backrole.Core.Internals;
using System.Collections.Generic;
using System.Linq;

namespace Backrole.Core.Builders
{
    /// <summary>
    /// Manipulates the configuration that is for the container.
    /// </summary>
    public partial class ConfigurationBuilder : IConfigurationBuilder
    {
        private Dictionary<string, ConfigurationBuilder> m_Subsets = new();
        private Dictionary<string, string> m_KeyValues = new();

        /// <summary>
        /// Initialize a new <see cref="ConfigurationBuilder"/> instance.
        /// </summary>
        public ConfigurationBuilder() => Preview = new Previews(this);

        /// <inheritdoc/>
        public IConfiguration Preview { get; }

        /// <summary>
        /// Split the key by the collon like: {Key}:{SubKey}.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Remainder"></param>
        /// <returns></returns>
        private static string Split(string Key, out string Remainder)
        {
            var Index = Key.IndexOf(':');
            if (Index < 0)
            {
                Remainder = null;
                return Key;
            }

            Remainder = Key.Substring(Index + 1);
            return Key.Substring(0, Index);
        }

        /// <inheritdoc/>
        public IConfigurationBuilder Remove(string Key)
        {
            Key = Split(Key, out var _);

            m_Subsets.Remove(Key);
            m_KeyValues.Remove(Key);

            return this;
        }

        /// <inheritdoc/>
        private string Get(string Key, string Default = null)
        {
            Key = Split(Key, out var Subkey);

            if (Subkey != null)
            {
                if (!m_Subsets.TryGetValue(Key, out var Subset))
                    return Default;

                return Subset.Get(Subkey, Default);
            }

            if (m_KeyValues.TryGetValue(Key, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get all keys on the configuration builder.
        /// </summary>
        /// <param name="Prefix"></param>
        /// <returns></returns>
        private IEnumerable<string> GetKeys(string Prefix = null)
        {
            foreach(var Each in m_KeyValues.Keys)
            {
                if (Prefix != null)
                     yield return $"{Prefix}:{Each}";
                else yield return Each;
            }

            foreach(var Each in m_Subsets)
            {
                var Keybase = Prefix != null ? $"{Prefix}:{Each.Key}" : Each.Key;
                foreach (var Subkeys in Each.Value.GetKeys(Keybase))
                    yield return Subkeys;
            }
        }

        /// <inheritdoc/>
        public IConfigurationBuilder Set(string Key, string Value)
        {
            Key = Split(Key, out var Subkey);

            if (Subkey != null)
            {
                if (!m_Subsets.TryGetValue(Key, out var Subset))
                     m_Subsets[Key] = Subset = new ConfigurationBuilder();

                Subset.Set(Subkey, Value);
            }

            else
            {
                if (Value is null)
                    m_KeyValues.Remove(Key);

                else
                    m_KeyValues[Key] = Value;
            }

            return this;
        }

        /// <inheritdoc/>
        public IConfigurationBuilder Set(params KeyValuePair<string, string>[] KeyValues)
        {
            foreach (var Each in KeyValues)
                Set(Each.Key, Each.Value);

            return this;
        }


        /// <inheritdoc/>
        public IConfigurationBuilder Subset(string Key) => Subset(Key, false);

        /// <summary>
        /// Find the subset or create it if not exists.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="NoCreate"></param>
        /// <returns></returns>
        public ConfigurationBuilder Subset(string Key, bool NoCreate)
        {
            Key = Split(Key, out var Subkey);

            if (!m_Subsets.TryGetValue(Key, out var Subset) && !NoCreate)
                m_Subsets[Key] = Subset = new ConfigurationBuilder();

            if (Subkey != null && Subset != null)
                return Subset.Subset(Subkey, NoCreate);

            return Subset;
        }

        /// <inheritdoc/>
        public IConfiguration Build()
        {
            var KeyValues = GetKeys()
                .Select(X => (Key: X, Value: Get(X)))
                .Where(X => X.Value != null);

            return new Configuration(KeyValues);
        }
    }
}
