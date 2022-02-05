using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Abstractions
{
    public interface IOrpMeshPeer
    {
        /// <summary>
        /// Indicates whether the peer is local initiated or not.
        /// </summary>
        bool IsLocalPeer { get; }

        /// <summary>
        /// Remote End Point.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Mesh token that received when handshaked.
        /// </summary>
        OrpMeshToken RemoteMeshToken { get; }

        /// <summary>
        /// State of the peer.
        /// </summary>
        OrpMeshPeerState State { get; }

        /// <summary>
        /// User Specific State instance.
        /// </summary>
        object UserState { get; set; }

        /// <summary>
        /// Emit a message to remote host asynchronously.
        /// </summary>
        /// <param name="Token"></param>
        /// <exception cref="OperationCanceledException">when the emitting has been canceled.</exception>
        /// <exception cref="InvalidOperationException">when the connection is not alive.</exception>
        /// <exception cref="ArgumentException">when the message's type hasn't mapped.</exception>
        /// <exception cref="ArgumentNullException">when the message is null.</exception>
        /// <exception cref="FormatException">when failed to pack the message.</exception>
        /// <returns></returns>
        Task<OrpMeshEmitStatus> EmitAsync(object Message, CancellationToken Token = default);
    }
}
