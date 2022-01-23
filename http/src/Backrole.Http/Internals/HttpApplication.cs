using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Internals.Opaques;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Internals
{
    internal class HttpApplication : IHttpApplication
    {
        private static readonly Func<Task> NO_NEXT = () => Task.CompletedTask;
        private Func<IHttpContext, Func<Task>, Task> m_App;

        /// <summary>
        /// Initialize a new <see cref="HttpApplication"/> instance.
        /// </summary>
        /// <param name="App"></param>
        public HttpApplication(Func<IHttpContext, Func<Task>, Task> App) => m_App = App;

        /// <summary>
        /// Invoke the application for <see cref="IHttpContext"/>.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(IHttpContext Context)
        {
            if (m_App is null)
                return;

            using var Aborting = new CancellationTokenSource();
            using var Scope = Context.Services.CreateScope(Services =>
            {
                Services.AddSingleton<IHttpContext>(Http =>
                {
                    var Services = Http.GetRequiredService<IHttpServiceProvider>();
                    return new HttpContextOpaque(Context, Services, Aborting.Token);
                });

                Services
                    .AddSingleton<IHttpRequest>(X => X.GetService<IHttpContext>().Request)
                    .AddSingleton<IHttpResponse>(X => X.GetService<IHttpContext>().Response);
            });

            try
            {
                using (Context.Aborted.Register(Aborting.Cancel))
                {
                    if (Debugger.IsAttached) 
                        await m_App(Scope.ServiceProvider.GetService<IHttpContext>(), NO_NEXT);

                    else
                    {
                        try { await m_App(Scope.ServiceProvider.GetService<IHttpContext>(), NO_NEXT); }
                        catch (Exception Exception)
                        {
                            Scope.ServiceProvider
                                .GetRequiredService<ILogger<IHttpApplication>>()
                                .Error("Failed to invoke the application.", Exception);
                        }
                    }
                }
            }
            finally
            {
                if (!Aborting.IsCancellationRequested)
                     Aborting.Cancel();
            }
        }
    }
}
