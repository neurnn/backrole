using Backrole.Orp.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Orp.Meshes.Internals.A_Messages
{
    [OrpMesh][OrpMessage]
    internal class INIT_Check : IOrpPackable, IOrpUnpackable
    {
        /// <summary>
        /// Phrase to be used to check the public key.
        /// </summary>
        public Guid Phrase { get; set; } = Guid.NewGuid();

        /// <inheritdoc/>
        public bool TryPack(BinaryWriter Output, IOrpReadOnlyOptions Options)
        {
            try { Output.Write(Phrase.ToByteArray()); }
            catch
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool TryUnpack(BinaryReader Input, IOrpReadOnlyOptions Options)
        {
            try { Phrase = new Guid(Input.ReadBytes(16)); }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
