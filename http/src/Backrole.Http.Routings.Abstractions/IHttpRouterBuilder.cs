using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using System;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Abstractions
{
    /// <summary>
    /// Builds an <see cref="IHttpRouter"/> instance.
    /// </summary>
    public interface IHttpRouterBuilder
    {
        /// <summary>
        /// To share datas between router delegates.
        /// </summary>
        IServiceProperties Properties { get; }

        /// <summary>
        /// Adds a <paramref name="Delegate"/> to filter the <see cref="IHttpRouterContext"/>.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHttpRouterBuilder Use(Func<IHttpRouterContext, Func<Task>, Task> Delegate);

        /// <summary>
        /// Gets the router builder that maps endpoints to the specified path.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Subpath"></param>
        /// <returns></returns>
        IHttpRouterBuilder Map(string Path, Action<IHttpRouterBuilder> Subpath);

        /// <summary>
        /// Adds an endpoint factory that receives all request from the specified path.
        /// This receives all requests if no uppers can handle the context.
        /// Note that the Wildcards (asterisk, '*' for method) are finally executed after all endpoint handling routines has completed.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        IHttpRouterBuilder On(string Method, Func<IHttpContext, Task<IHttpRouterEndpoint>> Factory);

        /// <summary>
        /// Build the <see cref="IHttpRouter"/> instance.
        /// </summary>
        /// <returns></returns>
        IHttpRouter Build();
    }
}
