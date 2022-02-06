using Backrole.Crypto.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Crypto.Internals
{
    public abstract class DoubleHashWrapper<THash> : IHashAlgorithm where THash : IHashAlgorithm, new()
    {
        private IHashAlgorithm m_Algorithm;

        /// <summary>
        /// Initialize a new <see cref="DoubleHashWrapper"/> that uses <see cref="IHashAlgorithm"/> instance.
        /// </summary>
        /// <param name="Factory"></param>
        public DoubleHashWrapper() => m_Algorithm = new THash();

        /// <inheritdoc/>
        public int Size => m_Algorithm.Size;

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public HashValue Hash(ArraySegment<byte> Input)
        {
            var Hash = m_Algorithm.Hash(Input).Value;
            return new HashValue(Name, m_Algorithm.Hash(Hash).Value);
        }

        /// <inheritdoc/>
        public HashValue Hash(Stream Input)
        {
            var Hash = m_Algorithm.Hash(Input).Value;
            return new HashValue(Name, m_Algorithm.Hash(Hash).Value);
        }

        /// <inheritdoc/>
        public async Task<HashValue> HashAsync(Stream Input, CancellationToken Token = default)
        {
            var Hash = (await m_Algorithm.HashAsync(Input, Token)).Value;
            return new HashValue(Name, m_Algorithm.Hash(Hash).Value);
        }
    }
}
