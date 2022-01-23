using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1.Outputs
{
    internal class ChunkedOutputs : BaseOutputs
    {
        private static readonly byte[] EMPTY = new byte[0];

        private INovaStreamTransport m_Transport;
        private bool m_Completed;

        /// <summary>
        /// Initialize a new <see cref="ChunkedOutputs"/>
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Length"></param>
        public ChunkedOutputs(INovaStreamTransport Transport)
        {
            m_Transport = Transport;
            m_Completed = false;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(ArraySegment<byte> Source, CancellationToken Cancellation = default)
        {
            if (!m_Completed && !m_Transport.Completion.IsCompleted)
            {
                var Size = Encoding.ASCII.GetBytes(Source.Count.ToString("x"));
                var CrLf = Encoding.ASCII.GetBytes("\r\n");

                try
                {
                    await m_Transport.WriteAsync(Size);
                    await m_Transport.WriteAsync(CrLf);

                    if (Source.Count <= 0)
                        m_Completed = true;

                    else
                        await m_Transport.WriteAsync(Source);

                    await m_Transport.WriteAsync(CrLf);
                }

                catch
                {
                    await m_Transport.CloseAsync();
                    m_Completed = true;
                }
            }
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync() => new ValueTask(WriteAsync(EMPTY));

    }
}
