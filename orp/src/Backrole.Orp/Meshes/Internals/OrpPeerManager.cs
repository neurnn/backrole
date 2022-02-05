using Backrole.Orp.Abstractions;
using Backrole.Orp.Meshes.Internals.B_Remotes;
using Backrole.Orp.Meshes.Internals.C_Locals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Meshes.Internals
{
    class OrpPeerManager : IOrpMeshPeerManager
    {
        private OrpLocalPeerManager m_LocalPeers;
        private OrpRemotePeerManager m_RemotePeers;
        private List<IOrpMeshPeer> m_Peers = new();

        /// <summary>
        /// Initialize a new <see cref="OrpPeerManager"/> instance.
        /// </summary>
        /// <param name="Orp"></param>
        /// <param name="Server"></param>
        public OrpPeerManager(OrpMesh Orp, OrpServer Server)
        {
            m_LocalPeers = new OrpLocalPeerManager(Orp);
            m_RemotePeers = new OrpRemotePeerManager(Server, Orp);
            Mesh = Orp;

            m_LocalPeers.StatusChanged += (_, X) => OnStatusChanged(X);
            m_RemotePeers.StatusChanged += (_, X) => OnStatusChanged(X);
        }

        public void Start()
        {
            m_LocalPeers.RunWorker();
            m_RemotePeers.RunWorker();
        }

        public async ValueTask StopAsync()
        {
            await m_LocalPeers.DisposeAsync();
            await m_RemotePeers.GetWorker();
        }

        /// <summary>
        /// Proxies the event and makes a peer list.
        /// </summary>
        /// <param name="Peer"></param>
        private void OnStatusChanged(IOrpMeshPeer Peer)
        {
            if (Peer.State == OrpMeshPeerState.Pending)
            {
                lock (m_Peers)
                      m_Peers.Add(Peer);
            }

            else if (Peer.State == OrpMeshPeerState.Removed)
            {

                lock (m_Peers)
                      m_Peers.Remove(Peer);
            }

            StateChanged?.Invoke(this, Peer);
        }

        /// <inheritdoc/>
        public IOrpMesh Mesh { get; }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                lock (m_Peers)
                    return m_Peers.Count;
            }
        }

        /// <inheritdoc/>
        public event Action<IOrpMeshPeerManager, IOrpMeshPeer> StateChanged;

        /// <inheritdoc/>
        public IOrpMeshPeerManager Add(IPEndPoint RemoteEndPoint)
        {
            m_LocalPeers.AddPeer(RemoteEndPoint);
            return this;
        }

        /// <inheritdoc/>
        public IOrpMeshPeerManager Remove(IPEndPoint RemoteEndPoint)
        {
            if (!m_LocalPeers.RemovePeer(RemoteEndPoint))
                 m_RemotePeers.KillPeer(RemoteEndPoint);

            return this;
        }

        /// <inheritdoc/>
        public IEnumerable<IOrpMeshPeer> GetPeers()
        {
            lock (m_Peers)
                return m_Peers.ToArray();
        }

        /// <inheritdoc/>
        public IEnumerable<IOrpMeshPeer> GetPeers(Predicate<IOrpMeshPeer> Filter)
        {
            lock (m_Peers)
                return m_Peers.FindAll(Filter);
        }

        public Task<int> DiscoverAsync(CancellationToken Token = default)
        {
            throw new NotImplementedException();
        }

    }
}
