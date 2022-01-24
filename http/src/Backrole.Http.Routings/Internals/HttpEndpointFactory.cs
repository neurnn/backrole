using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Internals
{
    using FactoryDelegate = Func<IHttpContext, Task<IHttpRouterEndpoint>>;

    internal struct HttpEndpointFactory
    {
        private static Task<IHttpRouterEndpoint> NULL_ENDPOINT = Task.FromResult<IHttpRouterEndpoint>(null);

        private (string Method, FactoryDelegate Factory)[] m_Factories;
        private (string Name, IHttpRouter Router)[] m_Subrouters;

        /// <summary>
        /// Initialize a new <see cref="HttpEndpointFactory"/>.
        /// </summary>
        /// <param name="Factories"></param>
        /// <param name="PathMappings"></param>
        public HttpEndpointFactory(
            ICollection<KeyValuePair<string, FactoryDelegate>> Factories,
            ICollection<KeyValuePair<string, IHttpRouterBuilder>> PathMappings)
        {
            m_Factories = Factories.Select(X => (Method: X.Key, Factory: X.Value)).ToArray();
            m_Subrouters = PathMappings
                .Select(X => (Name: X.Key, Router: X.Value.Build()))
                .ToArray();
        }

        /// <summary>
        /// Invoke the path mappings and factory delegates.
        /// </summary>
        /// <param name="Http"></param>
        /// <returns></returns>
        public Task<IHttpRouterEndpoint> InvokeAsync(IHttpContext Http)
        {
            var State = Http.GetRouterState();

            /* Firstly, try to pass the control flow to subrouter. */
            if (State.PendingPathNames.TryPeek(out var Name))
            {
                var Router = m_Subrouters.Where(X => X.Name == Name)
                    .Select(X => X.Router).FirstOrDefault();

                if (Router != null)
                {
                    State.PathNames.Push(Name);
                    State.PendingPathNames.Dequeue();
                    return Router.RouteAsync(Http);
                }
            }

            /* And then, try to invoke the per-method endpoint. */
            else if (State.PendingPathNames.Count <= 0)
            {
                var Factory = m_Factories
                    .Where(X => X.Method.Equals(Http.Request.Method, StringComparison.OrdinalIgnoreCase))
                    .Select(X => X.Factory).FirstOrDefault();

                if (Factory != null)
                    return Factory(Http);

                else if (m_Factories.FirstOrDefault(X => X.Method != "*").Factory != null)
                    return Task.FromResult<IHttpRouterEndpoint>(new MethodNotAllowed());
            }

            /* Finally, invokes the wildcards. */
            var Wildcard = m_Factories.Where(X => X.Method == "*")
                .Select(X => X.Factory).FirstOrDefault();

            if (Wildcard != null)
                return Wildcard(Http);

            return NULL_ENDPOINT;
        }

        /// <summary>
        /// Generates the 405 Method Not Allowed response.
        /// </summary>
        private struct MethodNotAllowed : IHttpRouterEndpoint
        {
            public Task InvokeAsync(IHttpContext Http)
            {
                Http.Response.Status = 405;
                Http.Response.StatusPhrase = null;
                return Task.CompletedTask;
            }
        }
    }
}
