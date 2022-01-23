using Backrole.Core;
using Backrole.Core.Abstractions;
using Backrole.Core.Builders;
using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Internals.Builders
{
    internal class HttpContainerBuilder : ContainerBuilder<IHttpContainer>, IHttpContainerBuilder
    {
        private List<Action<IServiceCollection>> m_Services = new();
        private List<Action<IHttpContextTransportBuilder>> m_Transports = new();
        private List<Action<IConfiguration, IServiceProvider>> m_Options = new();
        private List<Action<IHttpApplicationBuilder>> m_Configures = new();

        /// <summary>
        /// Initialize a new <see cref="HttpContainerBuilder"/> instance.
        /// </summary>
        /// <param name="HostServices"></param>
        public HttpContainerBuilder(IServiceProvider HostServices) : base(HostServices)
            => this.ConfigureTransport(X => X.UseNova());

        /// <inheritdoc/>
        public IHttpContainerBuilder ConfigureServices(Action<IServiceCollection> Delegate)
        {
            m_Services.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        public IHttpContainerBuilder ConfigureTransport(Action<IHttpContextTransportBuilder> Delegate)
        {
            m_Transports.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        public IHttpContainerBuilder Configure<TOptions>(Action<IConfiguration, TOptions> Delegate) where TOptions : class
        {
            m_Options.Add((Configuration, Services) =>
            {
                var Options = Services.GetRequiredService<IOptions<TOptions>>();
                Delegate?.Invoke(Configuration, Options.Value);
            });

            return this;
        }

        /// <inheritdoc/>
        public IHttpContainerBuilder Configure(Action<IHttpApplicationBuilder> Delegate)
        {
            m_Configures.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        protected override IHttpContainer Build(IServiceProvider ContainerServices)
        {
            var Configurations = ContainerServices.GetRequiredService<IConfiguration>();

            foreach (var Each in m_Options)
                Each?.Invoke(Configurations, ContainerServices);

            return ContainerServices
                .GetRequiredService<IServiceInjector>()
                .Create(typeof(HttpContainer)) as HttpContainer;
        }

        /// <inheritdoc/>
        protected override void OnConfigureServices(IServiceCollection Services)
        {
            var Transports = new HttpContextTransportBuilder(HostServices, Services);

            Services
                .AddHostedServiceUnique<HttpContainerWorker>()
                .AddHierarchial<IHttpServiceProvider, HttpServiceProvider>()
                .AddSingleton<IHttpContextTransport>(Transports.Build)
                .AddSingleton<IHttpApplication>(HttpServices =>
                {
                    var Application = new HttpApplicationBuilder(HttpServices);

                    foreach (var Each in m_Configures)
                        Each?.Invoke(Application);

                    return Application.Build();
                });

            foreach (var Each in m_Transports)
                Each?.Invoke(Transports);

            foreach (var Service in m_Services)
                Service?.Invoke(Services);
        }
    }
}
