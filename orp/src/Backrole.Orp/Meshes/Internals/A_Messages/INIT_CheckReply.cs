using Backrole.Orp.Abstractions;
using Secp256k1Net;
using System;
using System.IO;

namespace Backrole.Orp.Meshes.Internals.A_Messages
{
    [OrpMesh][OrpMessage]
    internal class INIT_CheckReply : IOrpPackable, IOrpUnpackable
    {
        /// <summary>
        /// Signature.
        /// </summary>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Initialize a new <see cref="INIT_CheckReply"/>.
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="Check"></param>
        /// <returns></returns>
        public static INIT_CheckReply From(OrpMeshToken Token, INIT_Check Check)
        {
            using (var Secp = new Secp256k1())
            {
                if (!Token.Sign(Check.Phrase.ToByteArray(), out var Signature))
                    throw new InvalidOperationException("Invalid token specified.");

                return new INIT_CheckReply { Signature = Signature };
            }
        }

        /// <summary>
        /// Verify the signature is correct or not.
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="Check"></param>
        /// <returns></returns>
        public bool Verify(OrpMeshToken Token, INIT_Check Check)
        {
            try { return Token.Verify(Signature, Check.Phrase.ToByteArray()); }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryPack(BinaryWriter Output, IOrpReadOnlyOptions Options)
        {
            try
            {
                Output.Write(Signature.Length);
                Output.Write(Signature);
            }

            catch { return false; }
            return true;
        }

        /// <inheritdoc/>
        public bool TryUnpack(BinaryReader Input, IOrpReadOnlyOptions Options)
        {
            try
            {
                Signature = Input.ReadBytes(Input.ReadInt32());
            }

            catch { return false; }
            return true;
        }
    }
}
