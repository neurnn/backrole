using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Represents the client of the ORP protocol.
    /// </summary>
    public interface IOrpClient : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Indicates whether the connection is alive or not.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Indicates whether the connection is server-mode or not.
        /// </summary>
        bool IsServerMode { get; }

        /// <summary>
        /// User State.
        /// </summary>
        object UserState { get; set; }

        /// <summary>
        /// Options of the ORP protocol.
        /// </summary>
        IOrpReadOnlyOptions Options { get; }

        /// <summary>
        /// Remote End Point of the connection.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Event that notifies all <see cref="IOrpClient"/> status changes.
        /// This never raised if the connection is from the <see cref="IOrpServer"/> instance.
        /// </summary>
        event Action<IOrpClient> Connected;

        /// <summary>
        /// Event that notifies all <see cref="IOrpClient"/> status changes.
        /// This never raised if the connection is from the <see cref="IOrpServer"/> instance.
        /// </summary>
        event Action<IOrpClient> Disconnected;

        /// <summary>
        /// Connect to the remote host asynchronously.
        /// Returns true if connected successfully, otherwise, the token triggered so failed.
        /// </summary>
        /// <exception cref="InvalidOperationException">when the connection is alive or already in progress.</exception>
        /// <param name="RemoteEP"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task<bool> ConnectAsync(IPEndPoint RemoteEP, CancellationToken Token = default);

        /// <summary>
        /// Wait message from the remote host asynchronously.
        /// </summary>
        /// <exception cref="OperationCanceledException">when the emitting has been canceled.</exception>
        /// <exception cref="InvalidOperationException">when the connection is not alive.</exception>
        /// <exception cref="NotSupportedException">when the connection is server-mode.</exception>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task<OrpMessage> WaitAsync(CancellationToken Token = default);

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
        Task<OrpEmitStatus> EmitAsync(object Message, CancellationToken Token = default);

        /// <summary>
        /// Kick the connection asynchronously.
        /// </summary>
        /// <returns></returns>
        ValueTask DisconnectAsync();
    }
}
