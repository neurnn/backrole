using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Internals
{
    internal class HttpContextTransport : IHttpContextTransport, IAsyncDisposable
    {
        private Func<IHttpServiceProvider, IHttpContextTransport>[] m_Factories;
        private IHttpContextTransport[] m_Transports;
        private Task<IHttpContext>[] m_Accepters;

        private IHttpServiceProvider m_HttpServices;

        private ILogger<IHttpContextTransport> m_Logger;
        private CancellationTokenSource m_Stopping = new();

        private Dictionary<IHttpContext, IHttpContextTransport> m_Traces = new();

        /// <summary>
        /// Initialize a new <see cref="HttpContextTransport"/> instance.
        /// </summary>
        /// <param name="Factories"></param>
        public HttpContextTransport(IServiceProvider Services, IEnumerable<Func<IHttpServiceProvider, IHttpContextTransport>> Factories)
        {
            m_Transports = new IHttpContextTransport[(m_Factories = Factories.ToArray()).Length];
            m_Accepters = new Task<IHttpContext>[m_Factories.Length];

            m_HttpServices = Services.GetRequiredService<IHttpServiceProvider>();
            m_Logger = Services.GetRequiredService<ILogger<IHttpContextTransport>>();
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken Cancellation = default)
        {
            var Count = 0;

            m_Logger.Trace("Creating Http transports...");
            for(var i = 0; i < m_Transports.Length; ++i)
            {
                if (m_Transports[i] is null)
                    m_Transports[i] = m_Factories[i].Invoke(m_HttpServices);
            }

            foreach (var Each in m_Transports)
            {
                if (Each is null)
                    continue;

                await Each.StartAsync(Cancellation);
                ++Count;
            }

            if (Count <= 0)
            {
                m_Logger.Fatal("No Http transports are configured.");
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            if (!m_Stopping.IsCancellationRequested)
                 m_Stopping.Cancel();

            for (var i = 0; i < m_Transports.Length; ++i)
            {
                var Each = m_Transports[i];
                if (Each is null)
                    continue;

                m_Transports[i] = null;
                await Each.StopAsync();
            }

            m_Logger.Trace("Waiting the accepting tasks are completed...");
            try { await Task.WhenAll(m_Accepters.Where(X => X != null)); }
            catch { }
        }

        /// <inheritdoc/>
        public async Task<IHttpContext> AcceptAsync(CancellationToken Cancellation = default)
        {
            if (m_Transports.Length == 0)
            {
                using (var Cts = CancellationTokenSource.CreateLinkedTokenSource(Cancellation, m_Stopping.Token))
                    await Task.Delay(Timeout.Infinite, Cts.Token);

                throw new OperationCanceledException();
            }

            if (m_Transports.Length == 1)
            {
                using (var Cts = CancellationTokenSource.CreateLinkedTokenSource(Cancellation, m_Stopping.Token))
                    return await m_Transports[0].AcceptAsync(Cts.Token);
            }

            while (true)
            {
                for (var i = 0; i < m_Accepters.Length; ++i)
                {
                    if (m_Accepters[i] is null && m_Transports[i] != null)
                        m_Accepters[i] = m_Transports[i].AcceptAsync(m_Stopping.Token);
                    if (m_Accepters[i] != null && m_Accepters[i].IsCompleted)
                    {
                        try
                        {
                            var Result = await m_Accepters[i];
                            m_Traces[Result] = m_Transports[i];
                            m_Accepters[i] = null;

                            return Result;
                        }

                        catch (Exception e)
                        {
                            m_Logger.Error("Failed to accept a context from transport due to exception.", e);
                            m_Accepters[i] = null;
                        }
                    }
                }

                Cancellation.ThrowIfCancellationRequested();
                if (m_Accepters.Count(X => X != null) > 0)
                {
                    m_Logger.Trace("Waiting the completion of accepting task...");

                    var Tcs = new TaskCompletionSource();
                    using (Cancellation.Register(Tcs.SetResult))
                        await Task.WhenAny(m_Accepters.Where(X => X != null).Append(Tcs.Task));
                }
            }
        }

        /// <inheritdoc/>
        public async Task CompleteAsync(IHttpContext Context)
        {
            if (m_Transports.Length == 1)
            {
                await m_Transports[0].CompleteAsync(Context);
                return;
            }

            if (!m_Traces.Remove(Context, out var Transport))
                throw new InvalidOperationException("The specified context instance isn't from the transport.");

            await Transport.CompleteAsync(Context);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            for(var i = 0; i < m_Transports.Length; ++i)
            {
                if (m_Transports[i] is IAsyncDisposable Async)
                    await Async.DisposeAsync();

                else if (m_Transports[i] is IDisposable Sync)
                    Sync.Dispose();
            }

            m_Stopping.Dispose();
        }
    }
}
