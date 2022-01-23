using Backrole.Http.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Abstractions
{
    /// <summary>
    /// Accepts incoming <see cref="IHttpContext"/> instance from the remote host.
    /// </summary>
    public interface INovaStream : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Indicates whether the <see cref="INovaStream"/> supports duplex mode or not.
        /// </summary>
        bool IsDuplexSupported { get; }

        /// <summary>
        /// Indicates whehter the HttpStream has upgraded to opaque or not.
        /// (Not for duplex mode)
        /// </summary>
        bool IsOpaqueUpgraded { get; }

        /// <summary>
        /// Native Stream of the connection.
        /// </summary>
        INovaStreamTransport Transport { get; }

        /// <summary>
        /// Accept an <see cref="IHttpContext"/> from the <see cref="INovaStream"/>.
        /// </summary>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        Task<IHttpContext> AcceptAsync(CancellationToken Cancellation = default);

        /// <summary>
        /// Complete the <see cref="IHttpContext"/> instance.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        Task CompleteAsync(IHttpContext Context);

        /// <summary>
        /// Upgrade this stream to the opacity stream from
        /// the <see cref="IHttpContext"/> and its underlying connection.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        Task<Stream> UpgradeToOpaqueStreamAsync(IHttpContext Context);
    }
}
