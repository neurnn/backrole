using Backrole.Orp.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Orp.Meshes.Internals.B_Remotes
{
    internal class OrpRemotePeerManager
    {
        private List<OrpRemotePeer> m_Peers = new();
        private OrpServer m_Server;
        private Task m_Worker;

        private OrpMesh m_Mesh;

        /// <summary>
        /// Initialize a new <see cref="OrpRemotePeerManager"/> instance.
        /// </summary>
        /// <param name="Server"></param>
        public OrpRemotePeerManager(OrpServer Server, OrpMesh Mesh)
        {
            m_Server = Server;
            m_Worker = Task.CompletedTask;
            m_Mesh = Mesh;

            Server.Connected += OnConnected;
            Server.Disconnected += OnDisconnected;
        }

        /// <summary>
        /// Run the worker that handles remote peers.
        /// </summary>
        public void RunWorker()
        {
            if (m_Worker.IsCompleted)
                m_Worker = RunAsync();
        }

        /// <summary>
        /// Get Worker task.
        /// </summary>
        /// <returns></returns>
        public Task GetWorker() => m_Worker;

        /// <summary>
        /// Event that notifies about the peer status changed.
        /// </summary>
        public event Action<OrpRemotePeerManager, OrpRemotePeer> StatusChanged;

        /// <summary>
        /// Kill all peers that is from the specified <see cref="IPEndPoint"/>.
        /// </summary>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public bool KillPeer(IPEndPoint EndPoint)
        {
            List<OrpRemotePeer> Peers;
            lock(m_Peers)
            {
                Peers = m_Peers.FindAll(X => X.RemoteEndPoint == EndPoint); 
            }

            if (Peers is null || Peers.Count <= 0)
                return false;

            foreach(var Each in Peers)
                Each.Connection.Dispose();

            return true;
        }

        /// <summary>
        /// Notify the <see cref="StatusChanged"/> event.
        /// </summary>
        /// <param name="Peer"></param>
        public void Notify(OrpRemotePeer Peer) => StatusChanged?.Invoke(this, Peer);

        /// <summary>
        /// Called when the connection arrived.
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="Connection"></param>
        private void OnConnected(IOrpServer Server, IOrpClient Connection)
        {
            OrpRemotePeer Peer = new OrpRemotePeer(this, Connection, m_Mesh);
            Connection.UserState = Peer;

            lock (m_Peers)
            {
                m_Peers.Add(Peer);
            }

            Peer.SetState(OrpMeshPeerState.Pending);
            Peer.SetState(OrpMeshPeerState.Connecting);
            Peer.SetState(OrpMeshPeerState.Handshaking);
        }

        /// <summary>
        /// Called when the connection disconnected.
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="Connection"></param>
        private void OnDisconnected(IOrpServer Server, IOrpClient Connection)
        {
            if (Connection.UserState is not OrpRemotePeer Peer)
                return;

            lock (m_Peers)
            {
                if (!m_Peers.Remove(Peer))
                    return;
            }

            Peer.SetState(OrpMeshPeerState.Disconnected);
            Peer.SetState(OrpMeshPeerState.Removed);
        }

        /// <summary>
        /// Run the <see cref="OrpRemotePeerManager"/> instance.
        /// </summary>
        /// <param name="Server"></param>
        /// <returns></returns>
        private async Task RunAsync()
        {
            while(true)
            {
                IOrpClient Connection;
                object Message;

                try
                {
                    var Data = await m_Server.WaitAsync();
                    if (Data.Message is null || Data.Source is null)
                        continue;

                    Connection = Data.Source;
                    Message = Data.Message;
                }
                catch
                {
                    break;
                }

                if (Connection.UserState is not OrpRemotePeer Peer)
                {
                    await Connection.DisposeAsync();
                    continue;
                }

                lock(this)
                {
                    if (!m_Peers.Contains(Peer))
                        continue;
                }

                await Peer.HandleAsync(Message);
            }
        }
    }
}
