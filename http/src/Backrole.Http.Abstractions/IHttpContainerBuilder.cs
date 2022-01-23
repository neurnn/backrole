using Backrole.Core.Abstractions;
using System;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Builds the Http Server Container.
    /// </summary>
    public interface IHttpContainerBuilder : IContainerBuilder
    {
        /// <summary>
        /// Adds a <paramref name="Delegate"/> that configures http container specific services to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHttpContainerBuilder ConfigureServices(Action<IServiceCollection> Delegate);

        /// <summary>
        /// Adds a <paramref name="Delegate"/> that configures the <see cref="IHttpContextTransport"/> that accepts the incoming <see cref="IHttpContext"/>s.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHttpContainerBuilder ConfigureTransport(Action<IHttpContextTransportBuilder> Delegate);

        /// <summary>
        /// Adds a delegate that configures the service options.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHttpContainerBuilder Configure<TOptions>(Action<IConfiguration, TOptions> Delegate) where TOptions : class;

        /// <summary>
        /// Adds <paramref name="Delegate"/> that configures the http application that hosted by the http container.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHttpContainerBuilder Configure(Action<IHttpApplicationBuilder> Delegate);
    }
}
