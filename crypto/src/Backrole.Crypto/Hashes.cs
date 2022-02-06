using Backrole.Crypto.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Backrole.Crypto
{
    /// <summary>
    /// Provides <see cref="IHashAlgorithm"/> instance that scanned from the <see cref="Assembly"/>.
    /// </summary>
    public sealed class Hashes : IHashAlgorithmProvider
    {
        private static readonly object[] EMPTY_ARGS = new object[0];
        private Dictionary<string, IHashAlgorithm> m_Algorithms = new();

        /// <summary>
        /// Initialize a new <see cref="Hashes"/> instance.
        /// </summary>
        public Hashes(Assembly Assembly)
        {
            if (Assembly is null)
                throw new ArgumentNullException($"Assembly should be specified to scan algorithms.");

            foreach(var Each in Assembly.GetTypes())
            {
                if (!Each.IsAssignableTo(typeof(IHashAlgorithm)) || Each.IsAbstract)
                    continue;

                var Instance = Each.GetConstructor(Type.EmptyTypes).Invoke(EMPTY_ARGS);
                if (Instance is not IHashAlgorithm Algorithm)
                    continue;

                m_Algorithms[Algorithm.Name.ToLower()] = Algorithm;
            }
        }

        /// <summary>
        /// Default <see cref="IHashAlgorithmProvider"/> Instance.
        /// </summary>
        public static Hashes Default { get; } = new Hashes(typeof(Hashes).Assembly);

        /// <inheritdoc/>
        public IEnumerable<string> Supports => m_Algorithms.Keys;

        /// <inheritdoc/>
        public IHashAlgorithm Get(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return null;

            m_Algorithms.TryGetValue(Name.ToLower(), out var Algorithm);
            return Algorithm;
        }

        /// <inheritdoc/>
        IAlgorithm IAlgorithmProvider.Get(string Name) => Get(Name);
    }

    
}
