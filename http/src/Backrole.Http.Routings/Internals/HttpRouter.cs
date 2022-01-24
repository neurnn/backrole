using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Internals
{
    internal class HttpRouter : IHttpRouter
    {
        private Func<IHttpRouterContext, Func<Task>, Task> m_Filter;
        private Func<IHttpContext, Task<IHttpRouterEndpoint>> m_Factory;

        public HttpRouter(
            Func<IHttpRouterContext, Func<Task>, Task> Filter,
            Func<IHttpContext, Task<IHttpRouterEndpoint>> Factory)
        {
            m_Filter = Filter;
            m_Factory = Factory;
        }

        /// <inheritdoc/>
        public async Task<IHttpRouterEndpoint> RouteAsync(IHttpContext Http)
        {
            var State = Http.GetRouterState();
            var Context = Http.GetRouterContext();

            State.Routers.Push(this);

            try { await m_Filter(Context, () => InvokeFactory(Http, Context)); }
            finally
            {
                State.Routers.Pop();
            }

            return Context.Endpoint;
        }

        private async Task InvokeFactory(IHttpContext Http, HttpRouterContext Context)
        {
            if (Context.Endpoint != null)
                return;

            Context.Endpoint = await m_Factory(Http);
        }
    }
}
