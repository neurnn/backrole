using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Drivers;
using Backrole.Http.Transports.Nova.Internals.Features;
using Backrole.Http.Transports.Nova.Internals.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals
{
    internal class NovaTransport : IHttpContextTransport
    {
        private Channel<IHttpContext> m_Channel = null;
        private INovaStreamListener m_Listener = null;
        private IHttpServiceProvider m_HttpServices;

        private IServiceScope m_TransportScope;
        private Task m_Accepter;

        private CancellationTokenSource m_Aborting = new();
        private NovaOptions m_Options;

        private ILogger<NovaTransport> m_Logger;

        /// <summary>
        /// Initialize a new <see cref="NovaTransport"/> instance.
        /// </summary>
        /// <param name="Options"></param>
        public NovaTransport(IHttpServiceProvider HttpServices, NovaOptions Options)
        {
            m_Listener = Options.Create(HttpServices);
            m_HttpServices = HttpServices;
            m_Options = Options;

            m_Logger = HttpServices.GetRequiredService<ILogger<NovaTransport>>();
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken Cancellation = default)
        {
            if (m_Aborting is null ||
                m_Aborting.IsCancellationRequested)
                m_Aborting = new CancellationTokenSource();

            m_Channel = Channel.CreateBounded<IHttpContext>(m_Options.Backlogs);
            m_TransportScope = m_HttpServices.CreateScope(Services =>
            {
                /* Registers the transport features. */
                Services
                    .AddScoped<IHttpOpaqueStreamFeature, NovaOpaqueStreamFeature>()
                    .AddScoped<IHttpWebSocketFeature, NovaWebSocketFeature>();
            });

            m_Listener.Start();
            m_Accepter = RunAsync(m_Aborting.Token);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            m_Channel.Writer.TryComplete();

            m_Listener.Stop();
            if (!m_Aborting.IsCancellationRequested)
                 m_Aborting.Cancel();

            await m_Accepter;
            await m_TransportScope.DisposeAsync();
            m_Aborting.Dispose();

            m_TransportScope = null;
        }

        /// <inheritdoc/>
        private async Task RunAsync(CancellationToken Cancellation)
        {
            var Running = true;

            using (Cancellation.Register(() => Running = false))
            {
                m_Logger.Info($"Nova Http Endpoint started: {m_Options.LocalAddress} ({m_Options.LocalPort}).");

                while (Running)
                {
                    INovaStream Stream;

                    var Http = m_TransportScope.ServiceProvider.GetService<IHttpServiceProvider>();
                    try { Stream = await m_Listener.AcceptAsync(Http); }
                    catch (Exception e)
                    {
                        if (e is EndOfStreamException)
                            break;

                        m_Logger.Error("Failed to accept a HTTP context due to exception.", e);
                        continue;
                    }

                    if (Stream.IsDuplexSupported)
                        _ = new DuplexDriver(Stream).RunAsync(m_Channel.Writer, Cancellation);

                    else
                        _ = new SimplexDriver(Stream).RunAsync(m_Channel.Writer, Cancellation);
                }

                m_Logger.Info($"Stopped to accept incomming Http requests from: {m_Options.LocalAddress} ({m_Options.LocalPort}).");
            }
        }

        /// <inheritdoc/>
        public Task<IHttpContext> AcceptAsync(CancellationToken Cancellation = default)
            => m_Channel.Reader.ReadAsync(Cancellation).AsTask();

        /// <inheritdoc/>
        public Task CompleteAsync(IHttpContext Context)
        {
            if (Context is NovaHttpContext Nova)
                return Nova.Stream.CompleteAsync(Context);

            throw new InvalidOperationException("the context isn't created for the NovaTransport.");
        }
    }
}
