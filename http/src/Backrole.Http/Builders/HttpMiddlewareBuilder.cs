using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using Backrole.Http.Abstractions;
using Backrole.Http.Internals.Endpoints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Builders
{
    using MiddlewareDelegate = Func<IHttpContext, Func<Task>, Task>;
    using MiddlewareCombine = HttpDelegateCombine<IHttpContext>;

    public class HttpMiddlewareBuilder : IHttpMiddlewareBuilder
    {
        private static Task EMPTY(IHttpContext Http, Func<Task> Next) => Next();
        private MiddlewareDelegate m_Middleware;

        /// <summary>
        /// Initialize a new <see cref="HttpMiddlewareBuilder"/> instance.
        /// </summary>
        /// <param name="HttpServices"></param>
        /// <param name="Properties"></param>
        public HttpMiddlewareBuilder(IHttpServiceProvider HttpServices, IServiceProperties Properties = null)
        {
            this.HttpServices = HttpServices;
            this.Properties = Properties ?? new ServiceProperties();

            Configurations = HttpServices.GetRequiredService<IConfiguration>();
        }

        /// <inheritdoc/>
        public IHttpServiceProvider HttpServices { get; }

        /// <inheritdoc/>
        public IServiceProperties Properties { get; }

        /// <inheritdoc/>
        public IConfiguration Configurations { get; }

        /// <inheritdoc/>
        public IHttpMiddlewareBuilder Use(MiddlewareDelegate Middleware)
        {
            if (m_Middleware != null)
                 m_Middleware = new MiddlewareCombine(m_Middleware, Middleware).InvokeAsync;
            else m_Middleware = Middleware;
            return this;
        }

        /// <inheritdoc/>
        IHttpApplicationBuilder IHttpApplicationBuilder.Use(MiddlewareDelegate Middleware) => Use(Middleware);

        /// <inheritdoc/>
        public MiddlewareDelegate Build() => m_Middleware ?? EMPTY;
    }
}
