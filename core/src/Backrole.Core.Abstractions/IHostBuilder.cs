using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interface that builds the <see cref="IHost"/> instance.
    /// </summary>
    public interface IHostBuilder
    {
        /// <summary>
        /// A central location to share data between service instance registrations. 
        /// </summary>
        IServiceProperties Properties { get; }

        /// <summary>
        /// Adds a delegate that manipulates the host configurations.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHostBuilder ConfigureConfigurations(Action<IConfigurationBuilder> Delegate);

        /// <summary>
        /// Adds a delegate that configures the loggings.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHostBuilder ConfigureLoggings(Action<ILoggerFactoryBuilder> Delegate);

        /// <summary>
        /// Adds a delegate that configures the host services.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHostBuilder ConfigureServices(Action<IServiceCollection> Delegate);

        /// <summary>
        /// Adds a delegate that configures the service options.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHostBuilder Configure<TOptions>(Action<IConfiguration, TOptions> Delegate) where TOptions : class;

        /// <summary>
        /// Adds a delegate that configure the container to <see cref="IHost"/> instance.
        /// </summary>
        /// <param name="Factory"></param>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHostBuilder Configure(IContainerBuilderFactory Factory, Action<IContainerBuilder> Delegate = null);

        /// <summary>
        /// Adds a delegate that configures the service itself.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IHostBuilder Configure(Action<IServiceProvider> Delegate);

        /// <summary>
        /// Builds the <see cref="IHost"/> instance.
        /// </summary>
        /// <returns></returns>
        IHost Build();
    }
}
