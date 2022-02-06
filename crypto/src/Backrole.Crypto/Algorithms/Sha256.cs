using Backrole.Crypto.Internals;
using System.Security.Cryptography;

namespace Backrole.Crypto.Algorithms
{
    /// <summary>
    /// Provides SHA256 hash method.
    /// </summary>
    public class Sha256 : HashWrapper
    {
        public Sha256() : base(SHA256.Create)
        {
        }

        /// <inheritdoc/>
        public override int Size { get; } = 256 / 8;

        /// <inheritdoc/>
        public override string Name { get; } = "SHA256";
    }
}
