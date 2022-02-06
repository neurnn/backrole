using Backrole.Crypto.Internals;

namespace Backrole.Crypto.Algorithms
{
    public class Rsa4096 : RsaWrapper
    {
        public Rsa4096() : base(4096)
        {
        }

        /// <inheritdoc/>
        public override string Name { get; } = "RSA4096";
    }
}
