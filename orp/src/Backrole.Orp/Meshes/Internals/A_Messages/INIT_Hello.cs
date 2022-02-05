using Backrole.Orp.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Orp.Meshes.Internals.A_Messages
{
    [OrpMesh][OrpMessage]
    internal class INIT_Hello : IOrpPackable, IOrpUnpackable
    {
        /// <summary>
        /// Network Id.
        /// </summary>
        public byte[] NetworkId { get; set; }

        /// <summary>
        /// Advertisement
        /// </summary>
        public IPEndPoint Advertisement { get; set; }

        /// <summary>
        /// Mesh Token.
        /// </summary>
        public OrpMeshToken MeshToken { get; set; }

        /// <summary>
        /// Initialize a new <see cref="INIT_Hello"/> that describes the <paramref name="Mesh"/>.
        /// </summary>
        /// <param name="Mesh"></param>
        /// <returns></returns>
        public static INIT_Hello From(OrpMesh Mesh) => new INIT_Hello
        {
             Advertisement = Mesh.Options.Advertisement,
             NetworkId = Mesh.Options.MeshNetworkId,
             MeshToken = Mesh.LocalMeshToken
        };

        /// <summary>
        /// Test whether the network id is compatible or not.
        /// </summary>
        /// <param name="Options"></param>
        /// <returns></returns>
        public bool IsNetworkCompatible(IOrpMeshReadOnlyOptions Options)
        {
            Span<byte> NetId = Options.MeshNetworkId;
            if (NetworkId != null && NetId.SequenceEqual(NetworkId))
                return true;

            return false;
        }

        /// <inheritdoc/>
        public bool TryPack(BinaryWriter Output, IOrpReadOnlyOptions Options)
        {
            if (NetworkId is null)
                return false;

            try
            {
                Output.Write((ushort) NetworkId.Length);
                Output.Write(NetworkId);

                if (Advertisement is null)
                    Output.Write("");

                else
                    Output.Write(Advertisement.ToString());

                if (!MeshToken.TryPack(Output, Options))
                    return false;
            }

            catch { return false; }
            return true;
        }

        /// <inheritdoc/>
        public bool TryUnpack(BinaryReader Input, IOrpReadOnlyOptions Options)
        {
            try
            {
                NetworkId = Input.ReadBytes(Input.ReadUInt16());

                var Temp = Input.ReadString();
                if (!string.IsNullOrWhiteSpace(Temp))
                    Advertisement = IPEndPoint.Parse(Temp);

                else
                    Advertisement = null;

                if (!(MeshToken = new OrpMeshToken()).TryUnpack(Input, Options))
                    return false;
            }

            catch { return false; }
            return true;
        }
    }
}
