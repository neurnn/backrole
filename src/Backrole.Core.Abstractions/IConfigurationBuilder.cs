using System.Collections.Generic;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Builds an <see cref="IConfiguration"/> instance.
    /// </summary>
    public interface IConfigurationBuilder
    {
        /// <summary>
        /// Preview of configurations.
        /// Note that this will not be used at runtime.
        /// </summary>
        IConfiguration Preview { get; }

        /// <summary>
        /// Branches the <see cref="IConfigurationBuilder"/> by the key.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        IConfigurationBuilder Subset(string Key);

        /// <summary>
        /// Set a value for the key.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        IConfigurationBuilder Set(string Key, string Value);

        /// <summary>
        /// Set multiple values to <see cref="IConfigurationBuilder"/>.
        /// </summary>
        /// <param name="KeyValue"></param>
        /// <returns></returns>
        IConfigurationBuilder Set(params KeyValuePair<string, string>[] KeyValues);

        /// <summary>
        /// Remove the key and its sub keys.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        IConfigurationBuilder Remove(string Key);

        /// <summary>
        /// Build an <see cref="IConfiguration"/> instance.
        /// </summary>
        /// <returns></returns>
        IConfiguration Build();
    }
}
