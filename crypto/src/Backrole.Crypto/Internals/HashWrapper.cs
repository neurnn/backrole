using Backrole.Crypto.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Crypto.Internals
{
    public abstract class HashWrapper : IHashAlgorithm
    {
        private Func<HashAlgorithm> m_Factory;

        /// <summary>
        /// Initialize a new <see cref="HashWrapper"/> that uses .NET cryptography implementation.
        /// </summary>
        /// <param name="Factory"></param>
        public HashWrapper(Func<HashAlgorithm> Factory) => m_Factory = Factory;

        /// <inheritdoc/>
        public abstract int Size { get; }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public HashValue Hash(ArraySegment<byte> Input)
        {
            using(var m_Algorithm = m_Factory())

            return new HashValue(Name, m_Algorithm.ComputeHash(Input.ToArray()));
        }

        /// <inheritdoc/>
        public HashValue Hash(Stream Input)
        {
            using (var m_Algorithm = m_Factory())
                return new HashValue(Name, m_Algorithm.ComputeHash(Input));
        }

        /// <inheritdoc/>
        public async Task<HashValue> HashAsync(Stream Input, CancellationToken Token = default)
        {
            using (var m_Algorithm = m_Factory())
                return new HashValue(Name, await m_Algorithm.ComputeHashAsync(Input, Token));
        }
    }
}
