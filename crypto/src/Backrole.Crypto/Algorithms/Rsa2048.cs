using Backrole.Crypto.Internals;

namespace Backrole.Crypto.Algorithms
{
    public class Rsa2048 : RsaWrapper
    {
        public Rsa2048() : base(2048)
        {
        }

        /// <inheritdoc/>
        public override string Name { get; } = "RSA2048";
    }
}
