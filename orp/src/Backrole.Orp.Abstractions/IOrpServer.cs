using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Accepts the ORP requests.
    /// </summary>
    public interface IOrpServer : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Indicates whether the <see cref="IOrpServer"/> has been started and accepting messages or not.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Local Endpoint to listen.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Options of the ORP protocol.
        /// </summary>
        IOrpReadOnlyOptions Options { get; }

        /// <summary>
        /// Event that notifies all <see cref="IOrpClient"/> status changes.
        /// </summary>
        event Action<IOrpServer, IOrpClient> Connected;

        /// <summary>
        /// Event that notifies all <see cref="IOrpClient"/> status changes.
        /// </summary>
        event Action<IOrpServer, IOrpClient> Disconnected;

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
        Task<OrpMessage> WaitAsync(CancellationToken Token = default);

        /// <summary>
        /// Broadcast a message to all remote hosts asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">when the server isn't started yet.</exception>
        /// <param name="Message"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task<OrpBroadcastStatus> BroadcastAsync(object Message, CancellationToken Token = default);
    }
}
