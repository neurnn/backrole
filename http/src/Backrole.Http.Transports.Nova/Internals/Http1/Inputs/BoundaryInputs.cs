using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1.Inputs
{
    internal class BoundaryInputs : BaseInputs
    {
        private INovaStreamTransport m_Transport;

        private bool m_Completed;
        private byte[] m_EndingBytes;

        private ArraySegment<byte>? m_FinalBytes;
        private byte[] m_Buffer;

        /// <summary>
        /// Initialize a new <see cref="LengthInputs"/> instance.
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Length"></param>
        public BoundaryInputs(INovaStreamTransport Transport, string Boundary)
        {
            m_Transport = Transport; m_Completed = false;
            m_EndingBytes = Encoding.ASCII.GetBytes($"--{Boundary}--");
            m_Buffer = new byte[Math.Max(m_EndingBytes.Length * 8, 2048)];
            m_FinalBytes = null;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(ArraySegment<byte> Dest, CancellationToken Cancellation)
        {
            var TotalLength = 0;
            if (!m_FinalBytes.HasValue)
            {
                while (!m_Completed && Dest.Count > 0)
                {
                    var Length = await m_Transport.PeekAsync(m_Buffer, Cancellation);
                    if (Length <= 0)
                    {
                        m_FinalBytes = null;
                        m_Completed = true;
                        break;
                    }

                    var Index = CheckEnding(m_Buffer, 0, Length);
                    if (Index < 0)
                    {
                        var Slice = Math.Min(Length - m_EndingBytes.Length, Dest.Count);
                        if (Slice > 0)
                        {
                            Buffer.BlockCopy(m_Buffer, 0, Dest.Array, Dest.Offset, Slice);
                            Dest = new ArraySegment<byte>(Dest.Array, Dest.Offset + Slice, Dest.Count - Slice);
                            m_Transport.AdvanceTo(Slice); TotalLength += Slice;
                        }

                        await m_Transport.BufferAsync(false, Cancellation);
                        continue;
                    }

                    var FinalBytes = new byte[Index + m_EndingBytes.Length];
                    Buffer.BlockCopy(m_Buffer, 0, FinalBytes, 0, FinalBytes.Length);

                    m_Transport.AdvanceTo(FinalBytes.Length);
                    m_FinalBytes = FinalBytes;
                    m_Completed = true;

                    TotalLength += await ReadAsync(Dest, Cancellation);
                    break;
                }

                return TotalLength;
            }

            while (m_FinalBytes.Value.Count > 0 && Dest.Count > 0)
            {
                var FinalBytes = m_FinalBytes.Value;
                var Slice = Math.Min(FinalBytes.Count, Dest.Count);
                if (Slice <= 0)
                    break;

                Buffer.BlockCopy(FinalBytes.Array, FinalBytes.Offset, Dest.Array, Dest.Offset, Slice);
                Dest = new ArraySegment<byte>(Dest.Array, Dest.Offset + Slice, Dest.Count - Slice);
                m_FinalBytes = new ArraySegment<byte>(FinalBytes.Array, FinalBytes.Offset + Slice, FinalBytes.Count - Slice);

                TotalLength += Slice;
            }

            return TotalLength;
        }

        /// <summary>
        /// Check the ending bytes as index from the buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Offset"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        private int CheckEnding(byte[] Buffer, int Offset, int Length)
        {
            if (Length < m_EndingBytes.Length)
                return -1;

            for(int i = 0, j = Offset; i <= Length - m_EndingBytes.Length; ++i, ++j)
            {
                var Found = true;

                for (var k = 0; k < m_EndingBytes.Length; ++k)
                {
                    if (Buffer[j + k] != m_EndingBytes[k])
                    {
                        Found = false;
                        break;
                    }
                }

                if (Found)
                    return j;
            }

            return -1;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            var Temp = new byte[16 * 1024];

            while (!m_Completed && !m_Transport.Completion.IsCompleted)
                await ReadAsync(Temp, default);
        }
    }
}
