using Backrole.Crypto.Internals;

namespace Backrole.Crypto.Algorithms
{
    public class Rsa1024 : RsaWrapper
    {
        public Rsa1024() : base(1024)
        {
        }

        /// <inheritdoc/>
        public override string Name { get; } = "RSA1024";
    }
}
