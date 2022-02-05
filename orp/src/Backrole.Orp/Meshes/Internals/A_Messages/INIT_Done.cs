using Backrole.Orp.Abstractions;
using System.IO;

namespace Backrole.Orp.Meshes.Internals.A_Messages
{
    [OrpMesh][OrpMessage]
    internal class INIT_Done : IOrpPackable, IOrpUnpackable
    {
        public bool TryPack(BinaryWriter Output, IOrpReadOnlyOptions Options) => true;
        public bool TryUnpack(BinaryReader Input, IOrpReadOnlyOptions Options) => true;
    }
}
