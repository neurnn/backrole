using Backrole.Http.Abstractions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Abstractions
{
    /// <summary>
    /// Routes the <see cref="IHttpContext"/>'s processing states.
    /// </summary>
    public interface IHttpRouter
    {
        /// <summary>
        /// Route the <see cref="IHttpContext"/> to correct <see cref="IHttpRouterEndpoint"/> instance.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        Task<IHttpRouterEndpoint> RouteAsync(IHttpContext Context);
    }
}
