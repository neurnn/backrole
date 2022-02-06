using Backrole.Crypto.Internals;

namespace Backrole.Crypto.Algorithms
{
    /// <summary>
    /// Provides Double SHA512 hash method.
    /// </summary>
    public class DSha512 : DoubleHashWrapper<Sha512>
    {
        /// <inheritdoc/>
        public override string Name => "DSHA512";
    }
}
