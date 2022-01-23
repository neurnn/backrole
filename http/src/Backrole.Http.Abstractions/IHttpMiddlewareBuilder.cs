using System;
using System.Threading.Tasks;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Builds the middleware that runs on the HTTP protocol.
    /// </summary>
    public interface IHttpMiddlewareBuilder : IHttpApplicationBuilder
    {
        /// <summary>
        /// Build the middleware.
        /// </summary>
        /// <returns></returns>
        Func<IHttpContext, Func<Task>, Task> Build();

        /// <summary>
        /// Adds a middleware delegate that handles the http request.
        /// </summary>
        /// <param name="Middleware"></param>
        /// <returns></returns>
        new IHttpMiddlewareBuilder Use(Func<IHttpContext, Func<Task>, Task> Middleware);
    }
}
