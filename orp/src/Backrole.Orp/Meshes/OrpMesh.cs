using Backrole.Orp.Abstractions;
using Backrole.Orp.Meshes.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Meshes
{
    public class OrpMesh : IOrpMesh
    {
        private OrpServer m_Server;
        private OrpPeerManager m_Peers;

        /// <summary>
        /// Initialize a new <see cref="OrpMesh"/> instance.
        /// </summary>
        /// <param name="LocalEndPoint"></param>
        public OrpMesh(IPEndPoint LocalEndPoint, IOrpMeshReadOnlyOptions Opts)
        {
            Options = Opts;

            m_Server = new OrpServer(Opts.ProtocolOptions, LocalEndPoint);
            m_Peers = new OrpPeerManager(this, m_Server);

            LocalMeshToken = OrpMeshToken.New();
            MessageBox = new OrpMessageBox();
        }

        /// <inheritdoc/>
        public IOrpMeshPeerManager Peers => m_Peers;

        /// <inheritdoc/>
        public IOrpMeshReadOnlyOptions Options { get; }

        /// <inheritdoc/>
        public IPEndPoint LocalEndPoint => m_Server.LocalEndPoint;

        /// <inheritdoc/>
        public OrpMeshToken LocalMeshToken { get; }

        /// <summary>
        /// Message Box.
        /// </summary>
        internal OrpMessageBox MessageBox { get; }

        /// <inheritdoc/>
        public bool Start(int Backlog = 64)
        {
            MessageBox.Start();

            if (m_Server.Start(Backlog))
            {
                m_Peers.Start();

                foreach (var Each in Options.InitialPeers)
                    m_Peers.Add(Each);

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void Stop() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public Task<OrpMeshMessage> WaitAsync(CancellationToken Token = default)
            => MessageBox.WaitAsync(Token);

        /// <inheritdoc/>
        public async Task<OrpMeshBroadcastStatus> BroadcastAsync(object Message, CancellationToken Token = default)
        {
            var Peers = m_Peers.GetPeers();
            var RealPeers = new List<IOrpMeshPeer>();
            var TimeStamp = DateTime.UtcNow;

            foreach(var Each in Peers.Where(X => X.State == OrpMeshPeerState.Connected))
            {
                try
                {
                    if (RealPeers.Find(X => X.RemoteMeshToken.Equals(Each.RemoteMeshToken)) != null)
                        continue;

                    var Emit = await Each.EmitAsync(Message, Token);
                    if (Emit.Destination != null && Emit.Message != null)
                        RealPeers.Add(Each);
                }
                catch
                {
                }
            }

            return new OrpMeshBroadcastStatus(RealPeers.ToArray(), TimeStamp, Message);
        }

        public void Dispose() => Stop();

        public async ValueTask DisposeAsync()
        {
            m_Server.Stop();
            await m_Peers.StopAsync();
            MessageBox.Stop();
        }

    }
}
