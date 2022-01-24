using Backrole.Http.Abstractions;
using System;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Abstractions
{
    /// <summary>
    /// A context in which the router can change or remove endpoints to return.
    /// </summary>
    public interface IHttpRouterContext
    {
        /// <summary>
        /// Http Context instance.
        /// </summary>
        IHttpContext HttpContext { get; }

        /// <summary>
        /// Endpoint to invoke.
        /// </summary>
        IHttpRouterEndpoint Endpoint { get; set; }
    }
}
