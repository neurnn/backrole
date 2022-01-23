using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Accepts the <see cref="IHttpContext"/> and completes them.
    /// </summary>
    public interface IHttpContextTransport
    {
        /// <summary>
        /// Start accepting <see cref="IHttpContext"/> from the remote http clients.
        /// </summary>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken Cancellation = default);

        /// <summary>
        /// Stop accepting <see cref="IHttpContext"/> and aborts all accepted <see cref="IHttpContext"/>s.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Accept an <see cref="IHttpContext"/> from the remote http clients.
        /// </summary>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        Task<IHttpContext> AcceptAsync(CancellationToken Cancellation = default);

        /// <summary>
        /// Complete the <see cref="IHttpContext"/> and sends its response to the remote http client.
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        Task CompleteAsync(IHttpContext Context);
    }
}
