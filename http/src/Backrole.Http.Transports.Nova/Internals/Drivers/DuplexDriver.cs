using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Drivers
{
    internal class DuplexDriver
    {
        private INovaStream m_NovaStream;

        /// <summary>
        /// Initialize a new <see cref="DuplexDriver"/> instance.
        /// </summary>
        /// <param name="NovaStream"></param>
        public DuplexDriver(INovaStream NovaStream)
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
                Task<IHttpContext> Accepter = null;
                List<Task> Executions = new();

                try
                {
                    while (
                        !m_NovaStream.Transport.Completion.IsCompleted &&
                        !m_NovaStream.IsOpaqueUpgraded)
                    {
                        if (Accepter is null)
                            Accepter = m_NovaStream.AcceptAsync(Cancellation);

                        if (Accepter.IsCompleted)
                        {
                            NovaHttpContext Context;

                            try { Context = (NovaHttpContext)await Accepter; }
                            catch { break; }
                            finally { Accepter = null; }

                            Context.Properties[typeof(INovaStream)] = m_NovaStream;
                            try
                            {
                                await Queue.WriteAsync(Context, Cancellation);
                                Executions.Add(Context.Response.WaitAsync(Cancellation));
                            }
                            catch
                            {
                                Executions.Add(CompleteWithError(Context));
                            }

                            continue;
                        }

                        await WhenAny(Tcs, Accepter, Executions);
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

                    try
                    {
                        await Accepter;
                    }
                    catch { }

                    while (Executions.Count > 0)
                        await HandleCompletions(Executions);
                }
            }
        }

        private static async Task WhenAny(TaskCompletionSource Tcs, Task<IHttpContext> Accepter, List<Task> Executions)
        {
            await Task.WhenAny(Executions.Append(Accepter).Append(Tcs.Task));
            await HandleCompletions(Executions);
        }

        private async Task CompleteWithError(NovaHttpContext Context)
        {
            Context.Response.Status = 503;
            Context.Response.StatusPhrase = "Service Unavailable";
            Context.Response.Headers.Clear();
            Context.Response.OutputStream = null;

            await m_NovaStream.CompleteAsync(Context);
        }

        private static async Task HandleCompletions(List<Task> Executions)
        {
            var Queue = new Queue<Task>(Executions.Where(X => X.IsCompleted));
            while (Queue.TryDequeue(out var TaskExecution))
            {
                Executions.Remove(TaskExecution);
                await TaskExecution;
            }
        }
    }
}
