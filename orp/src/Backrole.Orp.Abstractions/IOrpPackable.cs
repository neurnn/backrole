using System.IO;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Packs the object.
    /// </summary>
    public interface IOrpPackable
    {
        /// <summary>
        /// Encode the instance values to <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="Output"></param>
        /// <returns></returns>
        bool TryPack(BinaryWriter Output, IOrpReadOnlyOptions Options);
    }
}
