using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1.Inputs
{
    /// <summary>
    /// Read transport.
    /// </summary>
    internal class LengthInputs : BaseInputs
    {
        private INovaStreamTransport m_Transport;
        private int m_Length;

        /// <summary>
        /// Initialize a new <see cref="LengthInputs"/> instance.
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Length"></param>
        public LengthInputs(INovaStreamTransport Transport, int Length)
        {
            m_Transport = Transport;
            m_Length = Length;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(ArraySegment<byte> Dest, CancellationToken Cancellation)
        {
            var Length = 0;
            var TotalLength = 0;

            while ((Length = Math.Max(Math.Min(m_Length, Dest.Count), 0)) > 0 && Dest.Count > 0)
            {
                var Segment = new ArraySegment<byte>(Dest.Array, Dest.Offset, Length);
                var Slice = Math.Min(await m_Transport.PeekAsync(Segment, Cancellation), m_Length);
                if (Slice <= 0)
                {
                    break;
                }

                Dest = new ArraySegment<byte>(Dest.Array, Dest.Offset + Slice, Dest.Count - Slice);
                TotalLength += Slice;

                m_Length -= m_Transport.AdvanceTo(Slice);
            }

            return TotalLength;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            while (m_Length > 0 && !m_Transport.Completion.IsCompleted)
            {
                var Length = Math.Min(await m_Transport.BufferAsync(), m_Length);
                if (Length <= 0)
                    break;

                m_Length -= m_Transport.AdvanceTo(Length);
            }
        }
    }
}
