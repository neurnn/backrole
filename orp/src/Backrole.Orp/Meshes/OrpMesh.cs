using Backrole.Orp.Abstractions;
using Backrole.Orp.Meshes.Internals;
using Backrole.Orp.Meshes.Internals.A_Messages;
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
        public Task<OrpMeshBroadcastStatus> BroadcastAsync(object Message, CancellationToken Token = default)
        {
            return m_Peers.BroadcastAsync(Message, Token);
        }

        /// <inheritdoc/>
        public void Dispose() => Stop();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            m_Server.Stop();
            await m_Peers.StopAsync();
            MessageBox.Stop();
        }

    }
}
