using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Crypto.Abstractions
{
    /// <summary>
    /// Generates the signature of the input datas.
    /// </summary>
    public interface IHashAlgorithm : IAlgorithm
    {
        /// <summary>
        /// Size of the hash value in bytes.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Hash the <paramref name="Input"/> bytes.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        HashValue Hash(ArraySegment<byte> Input);

        /// <summary>
        /// Hash the entire <paramref name="Input"/> stream.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        HashValue Hash(Stream Input);

        /// <summary>
        /// Hash the entire <paramref name="Input"/> stream asynchronously.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task<HashValue> HashAsync(Stream Input, CancellationToken Token = default);
    }
}
