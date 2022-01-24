using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;

namespace Backrole.Http.Routings.Internals
{
    internal class HttpRouterContext : IHttpRouterContext
    {
        public HttpRouterContext(IHttpContext Http)
            => HttpContext = Http;

        /// <inheritdoc/>
        public IHttpContext HttpContext { get; }

        /// <inheritdoc/>
        public IHttpRouterEndpoint Endpoint { get; set; }
    }
}
