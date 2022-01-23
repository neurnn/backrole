using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using Backrole.Http.Abstractions;
using Backrole.Http.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Internals.Builders
{
    internal class HttpApplicationBuilder : IHttpApplicationBuilder
    {
        private HttpMiddlewareBuilder m_Middleware;

        /// <summary>
        /// Initialize a new <see cref="HttpApplicationBuilder"/> instance.
        /// </summary>
        /// <param name="HttpServices"></param>
        /// <param name="Properties"></param>
        public HttpApplicationBuilder(IServiceProvider HttpServices)
        {
            this.HttpServices = HttpServices.GetRequiredService<IHttpServiceProvider>();
            Configurations = HttpServices.GetRequiredService<IConfiguration>();

            m_Middleware = new HttpMiddlewareBuilder(this.HttpServices, Properties);
        }

        /// <inheritdoc/>
        public IHttpServiceProvider HttpServices { get; }

        /// <inheritdoc/>
        public IServiceProperties Properties { get; } = new ServiceProperties();

        /// <inheritdoc/>
        public IConfiguration Configurations { get; }

        /// <inheritdoc/>
        public IHttpApplicationBuilder Use(Func<IHttpContext, Func<Task>, Task> Middleware)
        {
            m_Middleware.Use(Middleware);
            return this;
        }

        /// <summary>
        /// Build the <see cref="IHttpApplication"/> instance.
        /// </summary>
        /// <returns></returns>
        internal IHttpApplication Build() => new HttpApplication(m_Middleware.Build());
    }

}
