using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1
{
    internal class NovaHttpStream : INovaStream
    {
        private static readonly Encoding HEADER_ENCODING = Encoding.ASCII;
        private CancellationTokenSource m_Aborting = new CancellationTokenSource();

        private IHttpServiceProvider m_Services;
        private Task<IHttpContext> m_Accepts;

        private NovaHttpInputStream m_LastInputs = null;
        private NovaHttpOutputStream m_LastOutputs = null;

        private bool m_OpaqueUpgraded = false;

        /// <summary>
        /// Initialize a new <see cref="NovaHttpStream"/> instance.
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Services"></param>
        public NovaHttpStream(INovaStreamTransport Transport, IHttpServiceProvider Services)
        {
            _ = (this.Transport = Transport).Completion.ContinueWith(_ =>
            {
                if (m_Aborting.IsCancellationRequested)
                    return;

                m_Aborting.Cancel();
            });

            m_Services = Services;
        }

        /// <inheritdoc/>
        public bool IsDuplexSupported => false;

        /// <inheritdoc/>
        public bool IsOpaqueUpgraded => m_OpaqueUpgraded;

        /// <inheritdoc/>
        public INovaStreamTransport Transport { get; }

        /// <inheritdoc/>
        public async Task<IHttpContext> AcceptAsync(CancellationToken Cancellation = default)
        {
            if (m_OpaqueUpgraded)
                throw new InvalidOperationException("This stream has been upgraded to opaque transport.");

            while (true)
            {
                if (Transport.Completion.IsCompleted)
                    throw new InvalidOperationException("The stream has been closed.");

                if (m_Accepts is null)
                    m_Accepts = InternalAcceptAsync();

                if (m_Accepts.IsCompleted)
                {
                    var RetVal = await m_Accepts;
                    m_Accepts = null;

                    if (RetVal is null)
                        continue;

                    return RetVal;
                }

                Cancellation.ThrowIfCancellationRequested();

                var Tcs = new TaskCompletionSource();
                using (Cancellation.Register(Tcs.SetResult))
                    await Task.WhenAny(m_Accepts, Tcs.Task);
            }
        }

        /// <summary>
        /// Prepare to accept new <see cref="IHttpContext"/> instance.
        /// </summary>
        /// <returns></returns>
        private async Task CleanupAsync()
        {
            try
            {
                if (m_LastInputs != null)
                    await m_LastInputs.DisposeAsync();

                m_LastInputs = null;
            }
            catch { }

            try
            {
                if (m_LastOutputs != null)
                    await m_LastOutputs.DisposeAsync();

                m_LastOutputs = null;
            }
            catch { }
        }

        /// <summary>
        /// Accept an <see cref="IHttpContext"/> instance asynchronously.
        /// </summary>
        /// <returns></returns>
        private async Task<IHttpContext> InternalAcceptAsync()
        {
            await CleanupAsync();
            await using (var Reader = Transport.ReadLines(HEADER_ENCODING, true))
            {
                if (!await Reader.MoveNextAsync())
                    return null;

                var MPV = Reader.Current.Split(' ', 3, StringSplitOptions.None);
                if (MPV.Length != 3)
                    return null;

                var Context = new NovaHttpContext()
                {
                    Services = m_Services,
                    Connection = Transport,
                    Aborted = m_Aborting.Token,
                    Stream = this
                };

                var Request = Context.Request = new NovaHttpRequest() { Context = Context };
                var Response = Context.Response = new NovaHttpResponse() { Context = Context };
                var Temp = MPV.Skip(1).First().Split('?', 2, StringSplitOptions.None);

                Request.Method = MPV.First();
                Request.PathString = Temp.First();
                Request.QueryString = Temp.Length > 1 ? Temp.Last() : "";
                Request.Protocol = MPV.Last();

                Context.Properties[typeof(NovaHttpContext)] = Context;

                if (!Request.Protocol.Equals("HTTP/1.1") &&
                    !Request.Protocol.Equals("HTTP/1.0"))
                {
                    await Transport.CloseAsync();
                    return null;
                }

                while (await Reader.MoveNextAsync())
                {
                    var Index = Reader.Current.IndexOf(':');
                    if (Index <= 0)
                    {
                        await Transport.CloseAsync();
                        return null;
                    }

                    var Name = Reader.Current.Substring(0, Index).TrimEnd();
                    var Value = Reader.Current.Substring(Index + 1).TrimStart();
                    if (string.IsNullOrWhiteSpace(Name))
                        continue;

                    Request.Headers.Add(new KeyValuePair<string, string>(Name, Value));
                }

                Response.Status = 200;
                Response.Headers.Set("Server", "Nova/Backrole.Http");

                Request.InputStream = m_LastInputs 
                    = new NovaHttpInputStream(Transport, Request);

                Response.OutputStream = Response.NovaStream = m_LastOutputs
                    = new NovaHttpOutputStream(Transport, Response);

                return Context;
            }
        }

        /// <inheritdoc/>
        public Task CompleteAsync(IHttpContext Context)
        {
            if (Context is NovaHttpContext Nova)
                return Nova.CompleteAsync();

            throw new InvalidOperationException("the given context isn't from this!");
        }

        /// <inheritdoc/>
        public async Task<Stream> UpgradeToOpaqueStreamAsync(IHttpContext Context)
        {
            if (!(Context is NovaHttpContext Nova))
                throw new InvalidOperationException("the given context isn't from this!");

            if (Context.Response.IsSent)
                throw new InvalidOperationException("The context has been completed.");

            m_OpaqueUpgraded = true;
            await Nova.CompleteAsync();

            return new NovaHttpStreamOpaque(Transport);
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (!m_Aborting.IsCancellationRequested)
                 m_Aborting.Cancel();

            await Transport.CloseAsync();
            await CleanupAsync();

            m_Aborting.Dispose();
        }
    }
}
