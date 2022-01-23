using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Internals.Builders
{
    internal class HttpContextTransportBuilder : IHttpContextTransportBuilder
    {
        private List<Func<IHttpServiceProvider, IHttpContextTransport>> m_Factories = new();
 
        /// <summary>
        /// Intiialize a new <see cref="HttpContextTransportBuilder"/>.
        /// </summary>
        /// <param name="HostServices"></param>
        /// <param name="HttpServices"></param>
        public HttpContextTransportBuilder(IServiceProvider HostServices, IServiceCollection HttpServices)
        {
            this.HostServices = HostServices;
            this.HttpServices = HttpServices;
        }

        /// <inheritdoc/>
        public IServiceProvider HostServices { get; }

        /// <inheritdoc/>
        public IServiceCollection HttpServices { get; }

        /// <inheritdoc/>
        public IHttpContextTransportBuilder Clear()
        {
            m_Factories.Clear();
            return this;
        }

        /// <inheritdoc/>
        public IHttpContextTransportBuilder Add(Func<IHttpServiceProvider, IHttpContextTransport> Delegate)
        {
            m_Factories.Add(Delegate);
            return this;
        }

        /// <summary>
        /// Build the <see cref="IHttpContextTransport"/> instance.
        /// </summary>
        /// <returns></returns>
        public IHttpContextTransport Build(IServiceProvider HttpServices) => new HttpContextTransport(HttpServices, m_Factories);
    }
}
