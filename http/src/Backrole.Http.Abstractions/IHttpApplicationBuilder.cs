using Backrole.Core.Abstractions;
using System;
using System.Threading.Tasks;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Builds the application that runs on the HTTP protocol.
    /// </summary>
    public interface IHttpApplicationBuilder
    {
        /// <summary>
        /// Http specific services.
        /// </summary>
        IHttpServiceProvider HttpServices { get; }

        /// <summary>
        /// A central location to share objects between http middleware delegates.
        /// </summary>
        IServiceProperties Properties { get; }

        /// <summary>
        /// Configurations.
        /// </summary>
        IConfiguration Configurations { get; }

        /// <summary>
        /// Adds a middleware delegate that handles the http request.
        /// </summary>
        /// <param name="Middleware"></param>
        /// <returns></returns>
        IHttpApplicationBuilder Use(Func<IHttpContext, Func<Task>, Task> Middleware);
    }
}
