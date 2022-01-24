using Backrole.Http.Abstractions;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Abstractions
{
    /// <summary>
    /// Implements the endpoint of the router.
    /// </summary>
    public interface IHttpRouterEndpoint
    {
        /// <summary>
        /// Invoke the <see cref="IHttpRouterEndpoint"/>.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        Task InvokeAsync(IHttpContext Context);
    }
}
