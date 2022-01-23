using Backrole.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Internals
{
    internal class HttpServiceProvider : IHttpServiceProvider
    {
        private IServiceProvider m_Services;

        /// <summary>
        /// Initialize a new <see cref="HttpServiceProvider"/>.
        /// </summary>
        /// <param name="Services"></param>
        public HttpServiceProvider(IServiceProvider Services)
            => m_Services = Services;

        /// <inheritdoc/>
        public object GetService(Type ServiceType)
            => m_Services.GetService(ServiceType);
    }
}
