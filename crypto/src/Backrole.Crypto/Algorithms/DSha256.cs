using Backrole.Crypto.Internals;

namespace Backrole.Crypto.Algorithms
{
    /// <summary>
    /// Provides Double SHA256 hash method.
    /// </summary>
    public class DSha256 : DoubleHashWrapper<Sha256>
    {
        /// <inheritdoc/>
        public override string Name => "DSHA256";
    }
}
