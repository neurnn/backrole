using System.IO;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Unpacks the object.
    /// </summary>
    public interface IOrpUnpackable
    {
        /// <summary>
        /// Decode the instance values from <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        bool TryUnpack(BinaryReader Input, IOrpReadOnlyOptions Options);
    }
}
