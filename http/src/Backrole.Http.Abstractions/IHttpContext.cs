using Backrole.Core.Abstractions;
using System.Threading;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Represents a model of the http request and response.
    /// </summary>
    public interface IHttpContext
    {
        /// <summary>
        /// A central location to share objects between middlewares.
        /// </summary>
        IServiceProperties Properties { get; }

        /// <summary>
        /// Http Services.
        /// </summary>
        IHttpServiceProvider Services { get; }

        /// <summary>
        /// Connection information.
        /// </summary>
        IHttpConnectionInfo Connection { get; }

        /// <summary>
        /// Request instance.
        /// </summary>
        IHttpRequest Request { get; }

        /// <summary>
        /// Response instance.
        /// </summary>
        IHttpResponse Response { get; }

        /// <summary>
        /// Triggered when the request has been aborted.
        /// </summary>
        CancellationToken Aborted { get; }
    }
}
