using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Abstractions
{
    public interface IOrpMesh : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Peers of the mesh.
        /// </summary>
        IOrpMeshPeerManager Peers { get; }

        /// <summary>
        /// Options of the mesh.
        /// </summary>
        IOrpMeshReadOnlyOptions Options { get; }

        /// <summary>
        /// Local End Point to listen incoming peers.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Local Mesh Token to advertise self.
        /// (This will be automatically generated in constructor)
        /// </summary>
        OrpMeshToken LocalMeshToken { get; }

        /// <summary>
        /// Start the <see cref="IOrpServer"/>.
        /// </summary>
        /// <param name="Backlog"></param>
        /// <returns></returns>
        bool Start(int Backlog = 64);

        /// <summary>
        /// Stop the <see cref="IOrpServer"/>.
        /// </summary>
        void Stop();

        /// <summary>
        /// Wait messages from the remote hosts asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">when the server isn't started yet.</exception>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task<OrpMeshMessage> WaitAsync(CancellationToken Token = default);

        /// <summary>
        /// Broadcast a message to all remote hosts asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">when the server isn't started yet.</exception>
        /// <param name="Message"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task<OrpMeshBroadcastStatus> BroadcastAsync(object Message, CancellationToken Token = default);
    }
}
