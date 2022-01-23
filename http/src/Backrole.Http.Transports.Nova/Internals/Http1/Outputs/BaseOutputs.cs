using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1.Outputs
{
    internal abstract class BaseOutputs : IAsyncDisposable
    {
        /// <summary>
        /// Write bytes to output stream.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        public abstract Task WriteAsync(ArraySegment<byte> Source, CancellationToken Cancellation = default);

        /// <summary>
        /// Dispose the <see cref="BaseOutputs"/> instance.
        /// </summary>
        /// <returns></returns>
        public abstract ValueTask DisposeAsync();

        /// <summary>
        /// Create an output driver for the response headers.
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Response"></param>
        /// <returns></returns>
        public static BaseOutputs Create(INovaStreamTransport Transport, IHttpResponse Response)
        {
            var LengthHeader = Response.Headers.GetValue("Content-Length");
            var TransferHeader = Response.Headers.GetValue("Transfer-Encoding");

            if (!string.IsNullOrWhiteSpace(TransferHeader))
            {
                Response.Headers.Remove("Content-Length");
                LengthHeader = null;
            }

            if (string.IsNullOrWhiteSpace(LengthHeader) || !long.TryParse(LengthHeader, out var Length))
            {
                Response.Headers.Remove("Content-Length");
                Response.Headers.Set("Transfer-Encoding", "chunked");
                return new ChunkedOutputs(Transport);
            }

            Response.Headers.Remove("Transfer-Encoding");
            return new LengthOutputs(Transport, Length);
        }
    }
}
