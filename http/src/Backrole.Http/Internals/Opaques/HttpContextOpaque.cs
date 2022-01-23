using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Internals.Opaques
{
    internal class HttpContextOpaque : IHttpContext
    {
        /// <summary>
        /// Initialize a new <see cref="HttpContextOpaque"/>.
        /// </summary>
        /// <param name="Opaque"></param>
        public HttpContextOpaque(
            IHttpContext Context,
            IHttpServiceProvider Services,
            CancellationToken Aborted)
        {
            this.Context = Context;
            this.Services = Services;
            this.Aborted = Aborted;

            Request = new HttpRequestOpaque(this);
            Response = new HttpResponseOpaque(this);
        }

        /// <summary>
        /// Original context.
        /// </summary>
        public IHttpContext Context { get; }

        /// <inheritdoc/>
        public IServiceProperties Properties => Context.Properties;

        /// <inheritdoc/>
        public IHttpServiceProvider Services { get; }

        /// <inheritdoc/>
        public IHttpConnectionInfo Connection => Context.Connection;

        /// <inheritdoc/>
        public IHttpRequest Request { get; }

        /// <inheritdoc/>
        public IHttpResponse Response { get; }

        /// <inheritdoc/>
        public CancellationToken Aborted { get; }
    }
}
