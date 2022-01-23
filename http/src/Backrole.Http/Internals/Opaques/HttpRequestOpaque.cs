using Backrole.Http.Abstractions;
using System.IO;

namespace Backrole.Http.Internals.Opaques
{
    internal class HttpRequestOpaque : IHttpRequest
    {
        private HttpContextOpaque m_Opaque;

        /// <summary>
        /// Initialize a new <see cref="HttpRequestOpaque"/>.
        /// </summary>
        /// <param name="Opaque"></param>
        public HttpRequestOpaque(HttpContextOpaque Opaque)
            => m_Opaque = Opaque;

        /// <inheritdoc/>
        public IHttpContext Context => m_Opaque;

        /// <inheritdoc/>
        public string Method
        {
            get => m_Opaque.Context.Request.Method;
            set => m_Opaque.Context.Request.Method = value;
        }

        /// <inheritdoc/>
        public string PathString
        {
            get => m_Opaque.Context.Request.PathString;
            set => m_Opaque.Context.Request.PathString = value;
        }

        /// <inheritdoc/>
        public string QueryString
        {
            get => m_Opaque.Context.Request.QueryString;
            set => m_Opaque.Context.Request.QueryString = value;
        }

        /// <inheritdoc/>
        public string Protocol => m_Opaque.Context.Request.Protocol;

        /// <inheritdoc/>
        public IHttpHeaderCollection Headers => m_Opaque.Context.Request.Headers;

        /// <inheritdoc/>
        public Stream InputStream
        {
            get => m_Opaque.Context.Request.InputStream;
            set => m_Opaque.Context.Request.InputStream = value;
        }
    }
}
