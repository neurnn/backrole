using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1
{
    internal class NovaHttpStreamListener : INovaStreamListener
    {
        private TcpListener m_Listener;
        private NovaOptions m_Options;
        private CancellationTokenSource m_Cts;


        /// <summary>
        /// Initialize a new <see cref="NovaHttpStreamListener"/> instance.
        /// </summary>
        /// <param name="Options"></param>
        public NovaHttpStreamListener(NovaOptions Options)
        {
            m_Listener = new TcpListener(Options.LocalAddress, Options.LocalPort);
            m_Options = Options;
        }

        /// <inheritdoc/>
        public NovaListenMode ListenMode { get; } = NovaListenMode.Http1;

        /// <inheritdoc/>
        public void Start()
        {
            lock (this)
            {
                if (m_Cts != null)
                    throw new InvalidOperationException("The listener has started.");

                m_Cts = new CancellationTokenSource();
                m_Listener.Start(m_Options.Backlogs);
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            lock (this)
            {
                if (m_Cts is null)
                    throw new InvalidOperationException("The listener hasn't started yet.");

                m_Listener.Stop();
                m_Cts.Cancel();

                m_Cts.Dispose();
                m_Cts = null;
            }
        }

        /// <inheritdoc/>
        public async Task<INovaStream> AcceptAsync(IHttpServiceProvider HttpServices)
        {
            var Running = true;
            lock (this)
            {
                if (m_Cts is null)
                    throw new InvalidOperationException("The listener hasn't started yet.");

                m_Cts.Token.Register(() => Running = false);
            }

            var Logger = HttpServices.GetRequiredService<ILogger<NovaHttpStreamListener>>();
            while (Running)
            {
                try
                {
                    var Newbie = await m_Listener.AcceptTcpClientAsync();
                    var Transport = new NovaStreamTransport(Newbie);
                    return new NovaHttpStream(Transport, HttpServices);
                }

                catch (Exception e)
                {
                    if (Running)
                        Logger.Error("Failed to accept Tcp connection due to exception.", e);
                }
            }

            throw new EndOfStreamException("The listener has been stopped.");
        }
    }
}
