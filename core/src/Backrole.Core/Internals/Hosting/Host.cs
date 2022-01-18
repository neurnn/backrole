using Backrole.Core.Abstractions;
using Backrole.Core.Internals.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Hosting
{
    internal class Host : IHost
    {
        private Stack<IHostedService> m_HostedServices = new();
        private ServiceDisposables m_Disposables = new();

        [ServiceInjection(Required = true, ServiceType = typeof(IHostLifetime))]
        private HostLifetime m_Lifetime = null;

        [ServiceInjection(Required = true, ServiceType = typeof(IContainerLocator))]
        private HostContainerLocator m_Locator = null;

        [ServiceInjection(Required = true)]
        private IServiceScope m_RootScope = null;

        [ServiceInjection]
        private IHostedService m_HostedService = null;

        [ServiceInjection]
        private ILogger<IHost> m_Logger = null;

        private int m_StoppingState = 0;

        /// <inheritdoc/>
        [ServiceInjection(Required = true)]
        public IServiceProvider Services { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<IContainer> Containers => m_Locator.All();

        /// <summary>
        /// Dispose if the host is not disposed.
        /// </summary>
        ~Host() => Dispose();

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken Token = default)
        {
            if (m_Lifetime.Stopped.IsCancellationRequested)
                throw new InvalidOperationException("the host instance is disposable.");

            m_Logger.Info($"Starting the host instance...");

            await StartAsync(m_HostedService, Token);
            foreach (var Each in Containers)
            {
                m_Disposables.Reserve(Each);

                var HostedService = Each.Services.GetService<IHostedService>();
                if (HostedService is null || HostedService == m_HostedService)
                    continue;

                await StartAsync(HostedService, Token);
            }

            m_Lifetime.OnStarted();
            if (m_HostedServices.Count <= 0)
            {
                m_Logger.Fatal("No hosted service configured.");
                m_Lifetime.Stop();
            }
        }

        /// <summary>
        /// Start the <see cref="IHostedService"/> asynchronously.
        /// </summary>
        /// <param name="HostedService"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        private async Task StartAsync(IHostedService HostedService, CancellationToken Token)
        {
            Token.ThrowIfCancellationRequested();

            if (HostedService != null)
            {
                lock (this)
                    m_HostedServices.Push(HostedService);

                await HostedService.StartAsync(Token);
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            if (Interlocked.CompareExchange(ref m_StoppingState, 1, 0) == 0)
                m_Logger.Info($"Stopping the host...");

            while (true)
            {
                IHostedService HostedService;

                lock(this)
                {
                    if (!m_HostedServices.TryPop(out HostedService))
                        break;
                }

                try { await HostedService.StopAsync(); }
                finally
                {
                    await StopAsync();
                }
            }

            if (Interlocked.CompareExchange(ref m_StoppingState, 2, 1) == 1)
            {
                m_Logger.Info($"Stopped the host...");
                m_Lifetime.OnStopped();
            }
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await m_Disposables.DisposeAsync();
            await m_RootScope.DisposeAsync();
        }
    }
}
