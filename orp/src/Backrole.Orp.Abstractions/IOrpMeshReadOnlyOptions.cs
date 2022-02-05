using System;
using System.Collections.Generic;
using System.Net;

namespace Backrole.Orp.Abstractions
{
    public interface IOrpMeshReadOnlyOptions
    {
        /// <summary>
        /// Protocol Options that is underlying of the Mesh Networking protocol.
        /// </summary>
        IOrpReadOnlyOptions ProtocolOptions { get; }

        /// <summary>
        /// Address to advertise to other peers.
        /// If null, this will not advertise itself.
        /// </summary>
        IPEndPoint Advertisement { get; }

        /// <summary>
        /// Mesh Network ID to identify the target network is compatible or not.
        /// </summary>
        byte[] MeshNetworkId { get; }

        /// <summary>
        /// Max Retries per peer.
        /// If the number of retries exceeds this value, the corresponding peer information is removed.
        /// </summary>
        int MaxRetriesPerPeer { get; }

        /// <summary>
        /// Initial Peers to connect.
        /// </summary>
        IReadOnlyList<IPEndPoint> InitialPeers { get; }

        /// <summary>
        /// Timeout to connect to the remote host.
        /// </summary>
        TimeSpan ConnectionTimeout { get; }

        /// <summary>
        /// Recovery delay on the disconnected peers who are local to remote.
        /// </summary>
        TimeSpan ConnectionRecoveryDelay { get; }
    }
}
