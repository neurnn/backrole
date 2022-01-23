using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1.Outputs
{
    internal class LengthOutputs : BaseOutputs
    {
        private static readonly byte[] EMPTY_8K = new byte[8 * 1024];

        private INovaStreamTransport m_Transport;
        private long m_Length;

        /// <summary>
        /// Initialize a new <see cref="LengthOutputs"/>
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Length"></param>
        public LengthOutputs(INovaStreamTransport Transport, long Length)
        {
            m_Transport = Transport;
            m_Length = Length;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(ArraySegment<byte> Source, CancellationToken Cancellation = default)
        {
            var Slice = (int) Math.Max(Math.Min(Math.Min(Source.Count, m_Length), int.MaxValue), 0);
            if (Slice > 0 && !m_Transport.Completion.IsCompleted)
            {
                var Segment = new ArraySegment<byte>(Source.Array, Source.Offset, Slice);
                await m_Transport.WriteAsync(Segment, Cancellation);
            }
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            while(m_Length > 0 && !m_Transport.Completion.IsCompleted)
            {
                var Slice = (int)Math.Min(EMPTY_8K.Length, m_Length);
                await m_Transport.WriteAsync(new ArraySegment<byte>(EMPTY_8K, 0, Slice));

                m_Length -= Slice;
            }
        }
    }
}
