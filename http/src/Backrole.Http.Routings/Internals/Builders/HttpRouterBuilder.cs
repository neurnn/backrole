using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Internals.Builders
{
    internal class HttpRouterBuilder : IHttpRouterBuilder
    {
        private Dictionary<string, Func<IHttpContext, Task<IHttpRouterEndpoint>>> m_Factories = new();
        private Dictionary<string, IHttpRouterBuilder> m_PathMappings = new();
        private Func<IHttpRouterContext, Func<Task>, Task> m_Filter;

        /// <summary>
        /// Null Filter that supplied when no filters configured.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="Next"></param>
        /// <returns></returns>
        private static Task NULL_FILTER(IHttpRouterContext _, Func<Task> Next) => Next();

        /// <summary>
        /// Initialize a new <see cref="HttpRouterBuilder"/> instance.
        /// </summary>
        /// <param name="Properties"></param>
        public HttpRouterBuilder(IServiceProperties Properties = null)
            => this.Properties = Properties ?? new ServiceProperties();

        /// <inheritdoc/>
        public IServiceProperties Properties { get; }

        /// <inheritdoc/>
        public IHttpRouterBuilder Use(Func<IHttpRouterContext, Func<Task>, Task> Delegate)
        {
            if (m_Filter != null)
                m_Filter = new HttpFilterCombine(m_Filter, Delegate).InvokeAsync;

            else
                m_Filter = Delegate;

            return this;
        }

        /// <inheritdoc/>
        public IHttpRouterBuilder On(string Method, Func<IHttpContext, Task<IHttpRouterEndpoint>> Factory)
        {
            if (m_Factories.TryGetValue(Method, out var Prev))
                m_Factories[Method] = new CombineEndpoint(Prev, Factory).InvokeAsync;

            else
                m_Factories[Method] = Factory;

            return this;
        }

        /// <summary>
        /// Combines two endpoint delegate to single one.
        /// </summary>
        private struct CombineEndpoint
        {
            private Func<IHttpContext, Task<IHttpRouterEndpoint>> m_Prev;
            private Func<IHttpContext, Task<IHttpRouterEndpoint>> m_Next;

            public CombineEndpoint(
                Func<IHttpContext, Task<IHttpRouterEndpoint>> Prev,
                Func<IHttpContext, Task<IHttpRouterEndpoint>> Next)
            {
                m_Prev = Prev;
                m_Next = Next;
            }

            [DebuggerHidden]
            public async Task<IHttpRouterEndpoint> InvokeAsync(IHttpContext Http)
            {
                var Endpoint = await m_Prev(Http);
                if (Endpoint is null)
                    return await m_Next(Http);

                return Endpoint;
            }
        }

        /// <inheritdoc/>
        public IHttpRouterBuilder Map(string _Path, Action<IHttpRouterBuilder> Subpath)
        {
            var Names = new Queue<string>(HttpRouterUtils.SplitPathToNames(_Path).ToArray());
            if (!Names.TryDequeue(out var Now))
            {
                Subpath(this);
                return this;
            }

            (m_PathMappings[Now] = new HttpRouterBuilder(Properties))
                .Map(string.Join('/', Names), Subpath);

            return this;
        }

        /// <inheritdoc/>
        public IHttpRouter Build() => new HttpRouter(m_Filter ?? NULL_FILTER, BuildFactory());

        /// <summary>
        /// Build an all-in-one factory delegate.
        /// </summary>
        /// <returns></returns>
        private Func<IHttpContext, Task<IHttpRouterEndpoint>> BuildFactory() 
            => new HttpEndpointFactory(m_Factories, m_PathMappings).InvokeAsync;
    }
}
