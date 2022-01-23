using Backrole.Core;
using Backrole.Core.Abstractions;
using Backrole.Core.Builders;
using Backrole.Http.Abstractions;
using Backrole.Http.Internals.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http
{
    public static class HttpContainerExtensions
    {
        /// <summary>
        /// Adds a delegate that adds a http container and configures it.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureHttpContainer(this IHostBuilder This, Action<IHttpContainerBuilder> Delegate) 
            => This.Configure<HttpContainerBuilder>(Delegate);
    }
}
