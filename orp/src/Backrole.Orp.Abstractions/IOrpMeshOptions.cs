using System;
using System.Collections.Generic;
using System.Net;

namespace Backrole.Orp.Abstractions
{
    public interface IOrpMeshOptions : IOrpMeshReadOnlyOptions
    {
        /// <summary>
        /// Protocol Options that is underlying of the Mesh Networking protocol.
        /// </summary>
        new IOrpOptions ProtocolOptions { get; set; }

        /// <summary>
        /// Address to advertise to other peers.
        /// If null, this will not advertise itself.
        /// </summary>
        new IPEndPoint Advertisement { get; set; }

        /// <summary>
        /// Mesh Network ID to identify the target network is compatible or not.
        /// </summary>
        new byte[] MeshNetworkId { get; set; }

        /// <summary>
        /// Max Retries per peer.
        /// If the number of retries exceeds this value, the corresponding peer information is removed.
        /// </summary>
        new int MaxRetriesPerPeer { get; set; }

        /// <summary>
        /// Initial Peers to connect.
        /// </summary>
        new IList<IPEndPoint> InitialPeers { get; }

        /// <summary>
        /// Timeout to connect to the remote host.
        /// </summary>
        new TimeSpan ConnectionTimeout { get; set; }

        /// <summary>
        /// Recovery delay on the disconnected peers who are local to remote.
        /// </summary>
        new TimeSpan ConnectionRecoveryDelay { get; set; }
    }
}
