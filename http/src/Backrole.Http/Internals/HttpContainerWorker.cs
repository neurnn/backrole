using Backrole.Core;
using Backrole.Core.Abstractions;
using Backrole.Core.Hosting;
using Backrole.Http.Abstractions;
using Backrole.Http.Internals.Opaques;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Internals
{
    internal class HttpContainerWorker : BackgroundService
    {
        [ServiceInjection(Required = true)]
        private IHttpApplication m_Application = null;

        [ServiceInjection(Required = true)]
        private IHttpContextTransport m_Transport = null;

        [ServiceInjection(Required = true)]
        private ILogger<IHttpContainer> m_Logger = null;

        private List<Task> m_Tasks = new();

        /// <summary>
        /// Run the <see cref="IHttpContainer"/> worker.
        /// </summary>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        protected override async Task RunAsync(CancellationToken Cancellation)
        {
            using var Stopping = new CancellationTokenSource();
            using var ___ = Cancellation.Register(Stopping.Cancel);

            m_Logger.Trace("Starting Http transports...");
            try
            {
                try { await m_Transport.StartAsync(Stopping.Token); }
                catch (OperationCanceledException)
                {
                    return;
                }

                m_Logger.Warn("the Http Container has been started successfully.");
                while (true)
                {
                    IHttpContext Context;
                    try   { Context = await m_Transport.AcceptAsync(Stopping.Token); }
                    catch { Context = null; }

                    if (Context is null)
                    {
                        if (Cancellation.IsCancellationRequested)
                            break;

                        continue;
                    }

                    m_Tasks.RemoveAll(X => X.IsCompleted);
                    m_Tasks.Add(InvokeAsync(Context));
                }
            }

            finally
            {
                if (!Stopping.IsCancellationRequested)
                     Stopping.Cancel();

                m_Logger.Trace("Stopping Http transports...");
                await m_Transport.StopAsync();

                while (m_Tasks.Count > 0)
                {
                    await Task.WhenAny(m_Tasks);
                    m_Tasks.RemoveAll(X => X.IsCompleted);
                }

                m_Logger.Warn("the Http Container has been stopped successfully.");
            }
        }

        /// <summary>
        /// Invoke the application.
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        private async Task InvokeAsync(IHttpContext Context)
        {
            try { await m_Application.InvokeAsync(Context); }
            finally
            {
                if (Debugger.IsAttached)
                    await m_Transport.CompleteAsync(Context);

                else
                {
                    try { await m_Transport.CompleteAsync(Context); }
                    catch(Exception Exception)
                    {
                        Context.Services
                            .GetRequiredService<ILogger<IHttpContainer>>()
                            .Error("Failed to complete the context.", Exception);
                    }
                }
            }
        }

    }
}
