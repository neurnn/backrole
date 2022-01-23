using Backrole.Http.Abstractions;
using Backrole.Http.Abstractions.Defaults;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Models
{
    internal class NovaHttpResponse : IHttpResponse
    {
        private TaskCompletionSource m_Tcs = new TaskCompletionSource();
        private bool m_Sent = false;

        /// <inheritdoc/>
        public IHttpContext Context { get; set; }

        /// <inheritdoc/>
        public bool IsSent
        {
            get
            {
                lock (this)
                    return m_Sent;
            }
        }

        /// <inheritdoc/>
        public int Status { get; set; }

        /// <inheritdoc/>
        public string StatusPhrase { get; set; }

        /// <inheritdoc/>
        public IHttpHeaderCollection Headers { get; } = new HttpHeaderCollection();

        /// <inheritdoc/>
        public Stream OutputStream { get; set; }

        /// <summary>
        /// Nova Stream that is for checking the user has replaced the body stream or not.
        /// </summary>
        public Stream NovaStream { get; set; }

        /// <summary>
        /// Try to set sent value.
        /// </summary>
        /// <returns></returns>
        public bool TrySetSent()
        {
            lock(this)
            {
                if (m_Sent)
                    return false;

                m_Sent = true;
                return true;
            }
        }

        /// <summary>
        /// Complete the response and sends the response.
        /// </summary>
        /// <returns></returns>
        public async Task CompleteAsync()
        {
            try { await NovaStream.DisposeAsync(); }
            finally
            {
                m_Tcs.TrySetResult();
            }
        }

        /// <summary>
        /// Wait the completion of the response.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitAsync(CancellationToken Cancellation)
        {
            var Tcs = new TaskCompletionSource();

            using (Cancellation.Register(Tcs.SetResult))
                await Task.WhenAny(m_Tcs.Task, Tcs.Task);

            Tcs.TrySetResult();
            return m_Tcs.Task.IsCompleted;
        }
    }
}
