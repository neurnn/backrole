using Backrole.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Hosting
{
    internal class HostedService : IHostedService
    {
        private Queue<Func<IServiceProvider, IHostedService>> m_ServiceFactories = new();
        private Stack<Task<IHostedService>> m_StartedServices = new();
        private bool m_Running = false;

        [ServiceInjection]
        private IHostLifetime m_Lifetime = null;

        [ServiceInjection]
        private IServiceProvider m_Services = null;

        /// <summary>
        /// Attach the hosted service.
        /// </summary>
        /// <param name="Delegate"></param>
        public void Push(Func<IServiceProvider, IHostedService> Delegate)
        {
            lock (this)
            {
                if (m_Running)
                {
                    if (m_Lifetime.Stopping.IsCancellationRequested)
                        return;

                    m_StartedServices.Push(StartQuietly(Delegate(m_Services)));
                    return;
                }

                m_ServiceFactories.Enqueue(Delegate);
            }
        }

        /// <summary>
        /// Start the <see cref="IHostedService"/> and returns it immediately.
        /// </summary>
        /// <param name="Service"></param>
        /// <returns></returns>
        private async Task<IHostedService> StartQuietly(IHostedService Service)
        {
            try { await Service.StartAsync(m_Lifetime.Stopping); }
            catch(OperationCanceledException) 
            {
            }

            return Service;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken Token = default)
        {
            while(true)
            {
                Func<IServiceProvider, IHostedService> Delegate;
                lock (this)
                {
                    if (!m_ServiceFactories.TryDequeue(out Delegate))
                        break;

                    m_Running = true;
                }

                Token.ThrowIfCancellationRequested();

                var Service = Delegate(m_Services);
                m_StartedServices.Push(Task.FromResult(Service));
                await Service.StartAsync(Token);
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            while(true)
            {
                Task<IHostedService> Task;
                lock(this)
                {
                    if (!m_StartedServices.TryPop(out Task))
                        break;
                }

                try { await (await Task).StopAsync(); }
                finally
                {
                    await StopAsync();
                }
            }
        }
    }
}
