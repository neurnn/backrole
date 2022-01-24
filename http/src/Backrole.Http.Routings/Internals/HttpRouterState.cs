using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Backrole.Http.Routings.Internals
{
    internal class HttpRouterState : IHttpRouterState
    {
        public HttpRouterState(IHttpContext Http)
        {
            var Names = HttpRouterUtils.SplitPathToNames(Http.Request.PathString);
            foreach (var Each in Names)
                PendingPathNames.Enqueue(Each);
        }

        public Stack<IHttpRouter> Routers { get; } = new();

        public Stack<string> PathNames { get; } = new();

        public Queue<string> PendingPathNames { get; } = new();

        /// <inheritdoc/>
        public IHttpRouter Current => Routers.LastOrDefault();

        /// <inheritdoc/>
        IEnumerable<IHttpRouter> IHttpRouterState.Routers => Routers;

        /// <inheritdoc/>
        IEnumerable<string> IHttpRouterState.PathNames => PathNames;

        /// <inheritdoc/>
        IEnumerable<string> IHttpRouterState.PendingPathNames => PendingPathNames;

        /// <inheritdoc/>
        public IDictionary<string, string> PathParameters { get; } = new Dictionary<string, string>();
    }
}
