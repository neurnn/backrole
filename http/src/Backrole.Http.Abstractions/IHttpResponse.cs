using System.IO;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Represents the response that is to be sent to the http client.
    /// </summary>
    public interface IHttpResponse
    {
        /// <summary>
        /// Http context instance.
        /// </summary>
        IHttpContext Context { get; }

        /// <summary>
        /// Indicates whether the response is sent or not.
        /// </summary>
        bool IsSent { get; }

        /// <summary>
        /// Status code.
        /// </summary>
        int Status { get; set; }

        /// <summary>
        /// Status phrase.
        /// </summary>
        string StatusPhrase { get; set; }

        /// <summary>
        /// Rsponse headers.
        /// </summary>
        IHttpHeaderCollection Headers { get; }

        /// <summary>
        /// Body Stream to transfer.
        /// </summary>
        Stream OutputStream { get; set; }
    }
}
