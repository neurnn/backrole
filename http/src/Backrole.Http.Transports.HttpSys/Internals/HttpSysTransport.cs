using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.HttpSys.Internals
{
    internal class HttpSysTransport : IHttpContextTransport
    {
        private HttpListener m_Listener;
        private Task<HttpListenerContext> m_Accepter;
        private IHttpServiceProvider m_HttpServices;

        private IServiceScope m_TransportScope;

        private CancellationTokenSource m_Aborting = new();

        /// <summary>
        /// Initialize a new <see cref="HttpSysTransport"/> instance.
        /// </summary>
        /// <param name="Options"></param>
        public HttpSysTransport(IHttpServiceProvider HttpServices, HttpSysOptions Options)
        {
            m_Listener = Options.Create();
            m_HttpServices = HttpServices;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken Cancellation = default)
        {
            if (m_Aborting is null ||
                m_Aborting.IsCancellationRequested)
                m_Aborting = new CancellationTokenSource();

            m_Listener.Start();
            m_TransportScope = m_HttpServices.CreateScope(Services =>
            {
                /* Registers the transport features. */
                Services
                    .AddScoped<IHttpWebSocketFeature, HttpSysWebSocketFeature>();
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            m_Listener.Stop();
            if (!m_Aborting.IsCancellationRequested)
                 m_Aborting.Cancel();

            await m_TransportScope.DisposeAsync();
            m_Aborting.Dispose();

            m_TransportScope = null;
        }

        /// <inheritdoc/>
        public async Task<IHttpContext> AcceptAsync(CancellationToken Cancellation = default)
        {
            while (true)
            {
                Cancellation.ThrowIfCancellationRequested();

                try
                {
                    if (m_TransportScope is null)
                        throw new InvalidOperationException("The transport hasn't started.");

                    if (m_Accepter is null)
                        m_Accepter = m_Listener.GetContextAsync();
                }

                catch
                {
                    m_Accepter = null;
                    throw new OperationCanceledException();
                }

                if (m_Accepter.IsCompleted)
                {
                    try
                    {
                        var Services = m_TransportScope.ServiceProvider
                            .GetRequiredService<IHttpServiceProvider>();

                        var Context = await m_Accepter; m_Accepter = null;
                        return new HttpSysContext(Context, Services, m_Aborting.Token);
                    }

                    catch
                    {
                        m_Accepter = null;
                        continue;
                    }
                }

                var Tcs = new TaskCompletionSource();
                using (Cancellation.Register(Tcs.SetResult))
                    await Task.WhenAny(Tcs.Task, m_Accepter);
            }
        }

        /// <inheritdoc/>
        public Task CompleteAsync(IHttpContext Context)
        {
            if (Context is HttpSysContext Http)
                return Http.OnCompleteAsync();

            return Task.CompletedTask;
        }
    }
}
