using Backrole.Crypto.Internals;
using System.Security.Cryptography;

namespace Backrole.Crypto.Algorithms
{
    /// <summary>
    /// Provides SHA384 hash method.
    /// </summary>
    public class Sha384 : HashWrapper
    {
        public Sha384() : base(SHA384.Create)
        {
        }

        /// <inheritdoc/>
        public override int Size { get; } = 384 / 8;

        /// <inheritdoc/>
        public override string Name { get; } = "SHA384";
    }
}
