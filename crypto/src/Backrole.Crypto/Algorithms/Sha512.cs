using Backrole.Crypto.Internals;
using System.Security.Cryptography;

namespace Backrole.Crypto.Algorithms
{
    /// <summary>
    /// Provides SHA512 hash method.
    /// </summary>
    public class Sha512 : HashWrapper
    {
        public Sha512() : base(SHA512.Create)
        {
        }

        /// <inheritdoc/>
        public override int Size { get; } = 512 / 8;

        /// <inheritdoc/>
        public override string Name { get; } = "SHA512";
    }
}
