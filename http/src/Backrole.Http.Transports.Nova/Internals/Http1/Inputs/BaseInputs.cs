using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1.Inputs
{
    internal abstract class BaseInputs : IAsyncDisposable
    {


        /// <summary>
        /// Read bytes from the transport.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        public abstract Task<int> ReadAsync(ArraySegment<byte> Buffer, CancellationToken Cancellation);

        /// <summary>
        /// Dispose the input asynchronously.
        /// </summary>
        /// <returns></returns>
        public abstract ValueTask DisposeAsync();

        /// <summary>
        /// Create the <see cref="BaseInputs"/> instance that is the correct way of body content.
        /// </summary>
        /// <returns></returns>
        public static BaseInputs Create(INovaStreamTransport Transport, IHttpRequest Request)
        {
            var ContentType = Request.Headers.GetValue("Content-Type");
            var ContentLength = Request.Headers.GetValue("Content-Length");

            if (ContentType != null && ContentType.Contains("boundary=", StringComparison.OrdinalIgnoreCase))
            {
                var BoundaryString = ContentType.Split(';')
                    .FirstOrDefault(X => X.Trim().StartsWith("boundary="))
                    .Split('=').LastOrDefault();

                if (BoundaryString is null)
                    return null;

                return new BoundaryInputs(Transport, BoundaryString);
            }

            else if (ContentLength != null && int.TryParse(ContentLength, out var Length))
                return new LengthInputs(Transport, Length);

            return new NullInputs();
        }

    }
}
