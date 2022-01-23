using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Models
{
    internal class NovaHttpContext : IHttpContext
    {
        private TaskCompletionSource m_Tcs = new TaskCompletionSource();

        /// <inheritdoc/>
        public IServiceProperties Properties { get; } = new ServiceProperties();

        /// <inheritdoc/>
        public IHttpServiceProvider Services { get; set; }

        /// <summary>
        /// Stream Transport.
        /// </summary>
        public INovaStreamTransport Connection { get; set; }

        /// <summary>
        /// Nova Stream instance.
        /// </summary>
        public INovaStream Stream { get; set; }

        /// <summary>
        /// Request Model.
        /// </summary>
        public NovaHttpRequest Request { get; set; }

        /// <summary>
        /// Response Model.
        /// </summary>
        public NovaHttpResponse Response { get; set; }

        /// <inheritdoc/>
        IHttpConnectionInfo IHttpContext.Connection => Connection;

        /// <inheritdoc/>
        IHttpRequest IHttpContext.Request => Request;

        /// <inheritdoc/>
        IHttpResponse IHttpContext.Response => Response;

        /// <inheritdoc/>
        public CancellationToken Aborted { get; set; }

        /// <summary>
        /// Complete the request and sned the response to remote host.
        /// </summary>
        /// <returns></returns>
        public Task CompleteAsync()
        {
            if (m_Tcs.TrySetResult())
            {
                return Response.CompleteAsync();
            }

            return m_Tcs.Task;
        }
    }
}
