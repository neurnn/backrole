using Backrole.Http.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Abstractions
{
    /// <summary>
    /// Receives bytes from the remote host or send bytes to it.
    /// </summary>
    public interface INovaStreamTransport : IHttpConnectionInfo, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Task that completed when the stream closed.
        /// </summary>
        Task Completion { get; }

        /// <summary>
        /// Peek received bytes from the <see cref="INovaStreamTransport"/>.
        /// To advance forward, the user should call <see cref="AdvanceTo(int)"/> method.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        Task<int> PeekAsync(ArraySegment<byte> Buffer, CancellationToken Cancellation = default);

        /// <summary>
        /// Receive bytes to the internal `Peek` buffer to advance forward without copying anything.
        /// And returns full length of peeked bytes. if enforce is true, this will receive once without checking buffer state.
        /// </summary>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        Task<int> BufferAsync(bool Enforce = false, CancellationToken Cancellation = default);

        /// <summary>
        /// Advance forward the receive-buffer and returns the length really advanced to.
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        int AdvanceTo(int Length);

        /// <summary>
        /// Read bytes from the <see cref="INovaStreamTransport"/> and advance the receive-buffer forward.
        /// Usually, this implemented with combination of <see cref="PeekAsync(ArraySegment{byte}, CancellationToken)"/> and <see cref="AdvanceTo(int)"/>.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        Task<int> ReadAsync(ArraySegment<byte> Buffer, CancellationToken Cancellation = default);

        /// <summary>
        /// Write the bytes to the <see cref="INovaStreamTransport"/>.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        Task WriteAsync(ArraySegment<byte> Buffer, CancellationToken Cancellation = default);

        /// <summary>
        /// Close the <see cref="INovaStreamTransport"/> asynchronously.
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}
