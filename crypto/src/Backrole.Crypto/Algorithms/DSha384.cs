using Backrole.Crypto.Internals;

namespace Backrole.Crypto.Algorithms
{
    /// <summary>
    /// Provides Double SHA384 hash method.
    /// </summary>
    public class DSha384 : DoubleHashWrapper<Sha384>
    {
        /// <inheritdoc/>
        public override string Name => "DSHA384";
    }
}
