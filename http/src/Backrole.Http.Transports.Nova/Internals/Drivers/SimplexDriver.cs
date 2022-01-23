using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Drivers
{
    internal class SimplexDriver
    {
        private INovaStream m_NovaStream;

        /// <summary>
        /// Initialize a new <see cref="SimplexDriver"/> instance.
        /// </summary>
        /// <param name="NovaStream"></param>
        public SimplexDriver(INovaStream NovaStream)
            => m_NovaStream = NovaStream;

        /// <summary>
        /// Run the driver asynchronously.
        /// </summary>
        /// <param name="Queue"></param>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        public async Task RunAsync(ChannelWriter<IHttpContext> Queue, CancellationToken Cancellation)
        {
            var Tcs = new TaskCompletionSource();
            using (Cancellation.Register(Tcs.SetResult))
            {
                try
                {
                    while (
                        !m_NovaStream.Transport.Completion.IsCompleted &&
                        !m_NovaStream.IsOpaqueUpgraded)
                    {
                        NovaHttpContext Context;

                        try { Context = (NovaHttpContext)await m_NovaStream.AcceptAsync(Cancellation); }
                        catch
                        {
                            break;
                        }

                        Context.Properties[typeof(INovaStream)] = m_NovaStream;
                        try { await Queue.WriteAsync(Context, Cancellation); }
                        catch
                        {
                            Context.Response.Status = 503;
                            Context.Response.StatusPhrase = "Service Unavailable";
                            Context.Response.Headers.Clear();
                            Context.Response.OutputStream = null;

                            await m_NovaStream.CompleteAsync(Context);
                            continue;
                        }


                        /* Wait the response is completed. */
                        await Context.Response.WaitAsync(Cancellation);
                    }

                    if (m_NovaStream.IsOpaqueUpgraded)
                    {
                        /* Then, holds the connection. */
                        await Task.WhenAny(m_NovaStream.Transport.Completion, Tcs.Task);
                    }
                }

                finally
                {
                    await m_NovaStream.DisposeAsync();
                }
            }
        }
    }
}
