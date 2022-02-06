using Backrole.Crypto.Internals;
using System.Security.Cryptography;

namespace Backrole.Crypto.Algorithms
{
    /// <summary>
    /// Provides MD5 hash method.
    /// </summary>
    public class Md5 : HashWrapper
    {
        public Md5() : base(MD5.Create)
        {
        }

        /// <inheritdoc/>
        public override int Size { get; } = 128 / 8;

        /// <inheritdoc/>
        public override string Name { get; } = "MD5";
    }
}
