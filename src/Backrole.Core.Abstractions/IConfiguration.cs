using System.Collections.Generic;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interface that stores application configuration information.
    /// </summary>
    public interface IConfiguration : IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// Gets a configuration value by its key.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        string this[string Key] { get; }

        /// <summary>
        /// Keys which set on the configuration.
        /// </summary>
        IEnumerable<string> Keys { get; }

        /// <summary>
        /// Test whether the key exists or not.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        bool Contains(string Key);

        /// <summary>
        /// Get a configuration value by its key with its fall-back default value.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        string Get(string Key, string Default = null);
    }
}
