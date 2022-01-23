using Backrole.Http.Abstractions;
using Backrole.Http.Abstractions.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.HttpSys.Internals
{
    internal class HttpSysResponse : IHttpResponse
    {
        private bool m_IsSent;

        private HttpListenerResponse m_Response;
        private Stream m_OutputStream;

        /// <summary>
        /// Initialize a new <see cref="HttpSysResponse"/> instance.
        /// </summary>
        /// <param name="Context"></param>
        public HttpSysResponse(HttpSysContext Context)
        {
            this.Context = Context;

            m_Response = Context.Context.Response;
            HttpSysRequest.CopyHeadersFrom(m_Response.Headers, Headers);

            Status = 404;
            

            try { m_Response.Headers.Remove("Server"); } catch { }
            Headers.Set("Server", "Http.Sys/Backrole.Http");
        }

        /// <inheritdoc/>
        public IHttpContext Context { get; }

        /// <inheritdoc/>
        public bool IsSent => NoThrow(() =>
        {
            lock (this)
                return m_IsSent;
        });

        /// <inheritdoc/>
        public int Status
        {
            get => NoThrow(() => m_Response.StatusCode);
            set => NoThrow(() =>
            {
                m_Response.StatusCode = value;
                return StatusPhrase = null;
            });
        }

        /// <inheritdoc/>
        public string StatusPhrase
        {
            get => NoThrow(() => m_Response.StatusDescription);
            set
            {
                NoThrow(() =>
                {
                    var Phrase = value;

                    if (string.IsNullOrWhiteSpace(Phrase))
                        HttpSysStatusCodes.Table.TryGetValue(Status, out Phrase);

                    m_Response.StatusDescription = Phrase ?? "Unknown";
                    return 0;
                });
            }
        }

        /// <inheritdoc/>
        public IHttpHeaderCollection Headers { get; } = new HttpHeaderCollection();

        /// <inheritdoc/>
        public Stream OutputStream
        {
            get => NoThrow(() =>
            {
                lock (this)
                {
                    if (!m_IsSent && m_OutputStream is null)
                    {
                        try { SetHeadersTo(Headers, m_Response.Headers); }
                        catch { }

                        m_OutputStream = m_Response.OutputStream;
                    }

                    return m_OutputStream;
                }
            });

            set => m_OutputStream = value;
        }

        /// <summary>
        /// Suppress all exceptions that can be caused during the execution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Fn"></param>
        /// <returns></returns>
        private T NoThrow<T>(Func<T> Fn)
        {
            try { return Fn(); }
            catch { }
            return default;
        }

        /// <summary>
        /// Set Response to output buffer.
        /// </summary>
        /// <param name="Headers"></param>
        /// <param name="ResponseHeaders"></param>
        private static void SetHeadersTo(IHttpHeaderCollection Headers, NameValueCollection ResponseHeaders)
        {
            ResponseHeaders.Clear();
            foreach (var Each in Headers.OrderBy(X => X.Key))
            {
                if (string.IsNullOrWhiteSpace(Each.Key) ||
                    string.IsNullOrWhiteSpace(Each.Value))
                    continue;

                ResponseHeaders.Add(Each.Key, Each.Value);
            }
        }

        /// <summary>
        /// Transfer the contents.
        /// </summary>
        /// <returns></returns>
        public async Task SendAsync()
        {
            lock(this)
            {
                if (m_IsSent)
                {
                    CloseOutputStream();
                    return;
                }

                m_IsSent = true;
            }

            try { SetHeadersTo(Headers, m_Response.Headers); }
            catch { }

            try
            {
                if (m_Response.OutputStream != m_OutputStream && m_OutputStream != null)
                    await CopyOutputStreamAsync();
            }
            catch { }

            try { m_Response.Close(); }
            catch { }
        }

        /// <summary>
        /// Copy the output stream to response stream asynchronously.
        /// </summary>
        /// <param name="BufferSize"></param>
        /// <returns></returns>
        private async Task CopyOutputStreamAsync(int BufferSize = 16 * 1024)
        {
            var Buffer = new byte[BufferSize];
            var Destination = m_Response.OutputStream;

            while (true)
            {
                var Length = await m_OutputStream.ReadAsync(Buffer, Context.Aborted);
                if (Length <= 0)
                    break;

                await Destination.WriteAsync(Buffer, 0, Length, Context.Aborted);
            }

            try { await Destination.FlushAsync(); }
            catch { }

            CloseOutputStream();
        }

        /// <summary>
        /// Close the output stream.
        /// </summary>
        private void CloseOutputStream()
        {
            lock (this)
            {
                if (m_OutputStream != null &&
                    m_Response.OutputStream != m_OutputStream)
                {
                    try { m_OutputStream.Close(); }
                    catch { }

                    m_OutputStream = null;
                }
            }
        }
    }

}
