using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using Backrole.Core.Internals.Hosting;
using System;
using System.Collections.Generic;

namespace Backrole.Core.Builders
{
    public class HostBuilder : IHostBuilder
    {
        private List<Action<IConfigurationBuilder>> m_Configurations = new();
        private List<Action<ILoggerFactoryBuilder>> m_Loggings = new();
        private List<Action<IServiceCollection>> m_Services = new();
        private List<Func<IServiceProvider, IContainer>> m_Containers = new();
        private List<Action<IConfiguration, IServiceProvider>> m_Options = new();
        private List<Action<IServiceProvider>> m_Configures = new();

        /// <summary>
        /// Initialize a new <see cref="HostBuilder"/> instance.
        /// </summary>
        public HostBuilder() => ConfigureLoggings(X => X.AddConsole());

        /// <inheritdoc/>
        public IServiceProperties Properties { get; } = new ServiceProperties();

        /// <inheritdoc/>
        public IHostBuilder ConfigureConfigurations(Action<IConfigurationBuilder> Delegate)
        {
            m_Configurations.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder ConfigureLoggings(Action<ILoggerFactoryBuilder> Delegate)
        {
            m_Loggings.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder ConfigureServices(Action<IServiceCollection> Delegate)
        {
            m_Services.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder Configure(IContainerBuilderFactory Factory, Action<IContainerBuilder> Delegate = null)
        {
            m_Containers.Add(Services =>
            {
                var Builder = Factory.Create(Services);
                Delegate?.Invoke(Builder);
                return Factory.Build(Builder);
            });

            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder Configure<TOptions>(Action<IConfiguration, TOptions> Delegate) where TOptions : class
        {
            m_Options.Add((Configuration, Services) =>
            {
                var Options = Services.GetRequiredService<IOptions<TOptions>>();
                Delegate?.Invoke(Configuration, Options.Value);
            });

            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder Configure(Action<IServiceProvider> Delegate)
        {
            m_Configures.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        public IHost Build()
        {
            var Configurations = BuildConfigurations();
            var Services = BuildServiceProvider(Configurations);

            foreach (var Each in m_Options)
                Each?.Invoke(Configurations, Services);

            PrepareContainers(Services);
            foreach (var Each in m_Configures)
                Each?.Invoke(Services);

            return Services
                .GetRequiredService<IServiceInjector>()
                .Create(typeof(Host)) as IHost;
        }

        /// <summary>
        /// Build the configurations.
        /// </summary>
        /// <returns></returns>
        private IConfiguration BuildConfigurations()
        {
            var Builder = new ConfigurationBuilder();
            foreach (var Each in m_Configurations)
                Each?.Invoke(Builder);

            return Builder.Build();
        }

        /// <summary>
        /// Build the service provider.
        /// </summary>
        /// <returns></returns>
        private IServiceProvider BuildServiceProvider(IConfiguration Configurations)
        {
            var Services = new ServiceCollection(Properties)
                .AddSingleton<IHostLifetime, HostLifetime>()
                .AddSingleton<IContainerLocator, HostContainerLocator>()
                .AddSingleton(Configurations);

            if (m_Loggings.Count > 0)
            {
                var Loggers = new LoggerFactoryBuilder(Services);

                foreach (var Each in m_Loggings)
                    Each?.Invoke(Loggers);

                Services.AddSingleton<ILoggerFactory>(Loggers.Build);
            }

            foreach (var Each in m_Services)
                Each?.Invoke(Services);

            return new ServiceProvider(Services);
        }

        /// <summary>
        /// Prepare all containers for the host.
        /// </summary>
        /// <param name="Services"></param>
        private void PrepareContainers(IServiceProvider Services)
        {
            var Locator = Services.GetRequiredService<IContainerLocator>();
            if (Locator is HostContainerLocator HCL)
            {
                foreach (var Each in m_Containers)
                    HCL.Containers.Add(Each(Services));

                HCL.OnReady();
            }
        }

    }
}
