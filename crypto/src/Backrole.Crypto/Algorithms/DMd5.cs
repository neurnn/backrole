using Backrole.Crypto.Internals;

namespace Backrole.Crypto.Algorithms
{
    /// <summary>
    /// Provides Double MD5 hash method.
    /// </summary>
    public class DMd5 : DoubleHashWrapper<Md5>
    {
        /// <inheritdoc/>
        public override string Name => "DMD5";
    }
}
