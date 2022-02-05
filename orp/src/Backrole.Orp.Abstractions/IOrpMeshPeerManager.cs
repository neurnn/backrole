using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Abstractions
{
    public interface IOrpMeshPeerManager
    {
        /// <summary>
        /// Mesh instance.
        /// </summary>
        IOrpMesh Mesh { get; }

        /// <summary>
        /// Count of peers.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Event that notifies changes of peers.
        /// </summary>
        event Action<IOrpMeshPeerManager, IOrpMeshPeer> StateChanged;

        /// <summary>
        /// Get all peers.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IOrpMeshPeer> GetPeers();

        /// <summary>
        /// Get peers with the filter.
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        IEnumerable<IOrpMeshPeer> GetPeers(Predicate<IOrpMeshPeer> Filter);

        /// <summary>
        /// Add the peer that connect to the remote host.
        /// </summary>
        /// <param name="RemoteEndPoint"></param>
        /// <returns></returns>
        IOrpMeshPeerManager Add(IPEndPoint RemoteEndPoint);

        /// <summary>
        /// Remove the peer that connected to the remote host.
        /// </summary>
        /// <param name="RemoteEndPoint"></param>
        /// <returns></returns>
        IOrpMeshPeerManager Remove(IPEndPoint RemoteEndPoint);

        /// <summary>
        /// Execute the discovery request manually.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task<int> DiscoverAsync(CancellationToken Token = default);

        /// <summary>
        /// Broadcast a message to all remote hosts asynchronously. (Only to directly connected peers)
        /// </summary>
        /// <exception cref="InvalidOperationException">when the server isn't started yet.</exception>
        /// <param name="Message"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task<OrpMeshBroadcastStatus> BroadcastAsync(object Message, CancellationToken Token = default);
    }
}
