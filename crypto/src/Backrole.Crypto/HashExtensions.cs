using Backrole.Crypto.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Crypto
{
    /// <summary>
    /// Provides `Hash` and `TryHash` shortcut methods for <see cref="IHashAlgorithmProvider"/> instances.
    /// </summary>
    public static class HashExtensions
    {
        /// <summary>
        /// Hash the <paramref name="Input"/> bytes using specified algorithm.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="IHashAlgorithmProvider"/>.</exception>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static HashValue Hash(this IHashAlgorithmProvider Hashes, string Name, ArraySegment<byte> Input)
        {
            var Hash = Hashes.Get(Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Name}");

            return Hash.Hash(Input);
        }

        /// <summary>
        /// Hash the <paramref name="Input"/> bytes using specified algorithm.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static bool TryHash(this IHashAlgorithmProvider Hashes, string Name, ArraySegment<byte> Input, out HashValue Output)
        {
            var Hash = Hashes.Get(Name);
            if (Hash != null)
            {
                Output = Hash.Hash(Input);
                return true;
            }

            Output = default;
            return false;
        }

        /// <summary>
        /// Hash the entire <paramref name="Input"/> stream.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="IHashAlgorithmProvider"/>.</exception>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static HashValue Hash(this IHashAlgorithmProvider Hashes, string Name, Stream Input)
        {
            var Hash = Hashes.Get(Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Name}");

            return Hash.Hash(Input);
        }

        /// <summary>
        /// Hash the entire <paramref name="Input"/> stream.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static bool TryHash(this IHashAlgorithmProvider Hashes, string Name, Stream Input, out HashValue Output)
        {
            var Hash = Hashes.Get(Name);
            if (Hash != null)
            {
                Output = Hash.Hash(Input);
                return true;
            }

            Output = default;
            return false;
        }

        /// <summary>
        /// Hash the entire <paramref name="Input"/> stream asynchronously.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="IHashAlgorithmProvider"/>.</exception>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static Task<HashValue> HashAsync(this IHashAlgorithmProvider Hashes, string Name, Stream Input, CancellationToken Token = default)
        {
            var Hash = Hashes.Get(Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Name}");

            return Hash.HashAsync(Input, Token);
        }
    }
}
