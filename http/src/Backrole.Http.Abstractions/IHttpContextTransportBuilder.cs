using Backrole.Core.Abstractions;
using System;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Configures the <see cref="IHttpContextTransport"/> instance to the <see cref="IHttpContainerBuilder"/>.
    /// </summary>
    public interface IHttpContextTransportBuilder
    {
        /// <summary>
        /// Host Services.
        /// </summary>
        IServiceProvider HostServices { get; }

        /// <summary>
        /// Container Services.
        /// </summary>
        IServiceCollection HttpServices { get; }

        /// <summary>
        /// Removes all registered transports.
        /// </summary>
        /// <returns></returns>
        IHttpContextTransportBuilder Clear();

        /// <summary>
        /// Adds a factory delegate that creates the <see cref="IHttpContextTransport"/>.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHttpContextTransportBuilder Add(Func<IHttpServiceProvider, IHttpContextTransport> Delegate);
    }
}
