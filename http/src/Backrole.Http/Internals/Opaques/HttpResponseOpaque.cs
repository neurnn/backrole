using Backrole.Http.Abstractions;
using System.IO;

namespace Backrole.Http.Internals.Opaques
{
    internal class HttpResponseOpaque : IHttpResponse
    {
        private HttpContextOpaque m_Opaque;

        /// <summary>
        /// Initialize a new <see cref="HttpRequestOpaque"/>.
        /// </summary>
        /// <param name="Opaque"></param>
        public HttpResponseOpaque(HttpContextOpaque Opaque)
            => m_Opaque = Opaque;

        /// <inheritdoc/>
        public IHttpContext Context => m_Opaque;

        /// <inheritdoc/>
        public bool IsSent => m_Opaque.Context.Response.IsSent;

        /// <inheritdoc/>
        public int Status
        {
            get => m_Opaque.Context.Response.Status;
            set => m_Opaque.Context.Response.Status = value;
        }

        /// <inheritdoc/>
        public string StatusPhrase
        {
            get => m_Opaque.Context.Response.StatusPhrase;
            set => m_Opaque.Context.Response.StatusPhrase = value;
        }

        /// <inheritdoc/>
        public IHttpHeaderCollection Headers => m_Opaque.Context.Response.Headers;

        /// <inheritdoc/>
        public Stream OutputStream
        {
            get => m_Opaque.Context.Response.OutputStream;
            set => m_Opaque.Context.Response.OutputStream = value;
        }
    }
}
