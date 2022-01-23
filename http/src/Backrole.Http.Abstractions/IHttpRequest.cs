using System.IO;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Represents the request from the http client.
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Http context instance.
        /// </summary>
        IHttpContext Context { get; }

        /// <summary>
        /// Request method.
        /// </summary>
        string Method { get; set; }

        /// <summary>
        /// Requested Path.
        /// </summary>
        string PathString { get; set; }

        /// <summary>
        /// Query String.
        /// </summary>
        string QueryString { get; set; }

        /// <summary>
        /// Protocol that sent the request.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Request headers.
        /// </summary>
        IHttpHeaderCollection Headers { get; }

        /// <summary>
        /// Request body.
        /// </summary>
        Stream InputStream { get; set; }

    }
}
