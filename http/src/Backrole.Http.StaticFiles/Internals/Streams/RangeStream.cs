using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.StaticFiles.Internals.Streams
{
    internal class RangeStream : Stream
    {
        private Stream m_Stream;
        private long m_Offset;
        private long m_Length;

        /// <summary>
        /// Initialize a new <see cref="RangeStream"/> instance.
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="Position"></param>
        /// <param name="Length"></param>
        public RangeStream(Stream Stream, long Position, long Length)
        {
            (m_Stream = Stream)
                .Position = m_Offset;

            m_Offset = Position;
            m_Length = Length;
        }

        /// <inheritdoc/>
        public override bool CanRead => m_Stream != null ? m_Stream.CanRead : false;

        /// <inheritdoc/>
        public override bool CanSeek => m_Stream != null ? m_Stream.CanSeek : false;

        /// <inheritdoc/>
        public override bool CanWrite => m_Stream != null ? m_Stream.CanWrite : false;

        /// <inheritdoc/>
        public override long Length => m_Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => Math.Min(Math.Max(m_Stream.Position - m_Offset, 0), m_Length);
            set => m_Stream.Position = Math.Max(Math.Min(value, m_Length), 0) + m_Offset;
        }

        /// <inheritdoc/>
        public override void Flush() => m_Stream.Flush();

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return Position = offset;

                case SeekOrigin.Current:
                    return Position += offset;

                case SeekOrigin.End:
                    return Position = m_Length + offset;

                default:
                    break;
            }

            return -1;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            count = (int)Math.Max(Math.Min(Position + count, m_Length) - Position, 0);
            return m_Stream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            count = (int)Math.Max(Math.Min(Position + count, m_Length) - Position, 0);
            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var Length = (int)Math.Max(Math.Min(Position + buffer.Length, m_Length) - Position, 0);
            return base.ReadAsync(buffer.Slice(0, Length), cancellationToken);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            var Length = (int)Math.Max(Math.Min(Position + buffer.Length, m_Length) - Position, 0);
            return base.Read(buffer.Slice(0, Length));
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            count = (int)Math.Max(Math.Min(Position + count, m_Length) - Position, 0);
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => m_Stream.SetLength(value);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            count = (int)Math.Max(Math.Min(Position + count, m_Length) - Position, 0);
            m_Stream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            count = (int)Math.Max(Math.Min(Position + count, m_Length) - Position, 0);
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var Length = (int)Math.Max(Math.Min(Position + buffer.Length, m_Length) - Position, 0);
            return base.WriteAsync(buffer.Slice(0, Length), cancellationToken);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            var Length = (int)Math.Max(Math.Min(Position + buffer.Length, m_Length) - Position, 0);
            base.Write(buffer.Slice(0, Length));
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            count = (int)Math.Max(Math.Min(Position + count, m_Length) - Position, 0);
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (m_Stream != null)
            {
                await m_Stream.DisposeAsync();
                m_Stream = null;
            }
        }
    }

}
