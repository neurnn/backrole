using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1
{
    internal class NovaHttpStreamOpaque : Stream
    {
        private INovaStreamTransport m_Transport;

        /// <summary>
        /// Initialize a new <see cref="NovaHttpStreamOpaque"/> instance.
        /// </summary>
        /// <param name="Transport"></param>
        public NovaHttpStreamOpaque(INovaStreamTransport Transport) 
            => m_Transport = Transport;

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override int Read(byte[] Buffer, int Offset, int Count)
            => ReadAsync(Buffer, Offset, Count, default).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override int Read(Span<byte> Dest)
        {
            var Temp = new byte[2048];
            var Slice = Math.Min(Temp.Length, Dest.Length);

            if (Slice <= 0)
                return 0;

            if ((Slice = Read(Temp, 0, Slice)) <= 0)
                return 0;

            new ArraySegment<byte>(Temp, 0, Slice)
                .AsSpan().CopyTo(Dest.Slice(0, Slice));

            return Slice;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> Dest, CancellationToken Cancellation = default)
        {
            var Temp = new byte[2048];
            var Slice = Math.Min(Temp.Length, Dest.Length);

            if (Slice <= 0)
                return 0;

            if ((Slice = await ReadAsync(Temp, 0, Slice, Cancellation)) <= 0)
                return 0;

            new ArraySegment<byte>(Temp, 0, Slice)
                .AsSpan().CopyTo(Dest.Span.Slice(0, Slice));

            return Slice;
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] Buffer, int Offset, int Count, CancellationToken Cancellation) 
            => m_Transport.ReadAsync(new ArraySegment<byte>(Buffer, Offset, Count), Cancellation);

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback Callback, object State)
        {
            var Task = ReadAsync(buffer, offset, count);
            Task.ContinueWith(X => ThreadPool.QueueUserWorkItem(_ => Callback?.Invoke(Task)));

            return new TaskToAsyncResult<int>
            {
                Task = Task,
                AsyncState = State
            };
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult IAR)
        {
            if (!(IAR is TaskToAsyncResult<int> Adapter))
                throw new InvalidOperationException("the async result isn't from the BeginRead method.");

            try { return Adapter.Task.GetAwaiter().GetResult(); }
            catch
            {
                return 0;
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] Buffer, int Offset, int Count)
            => WriteAsync(Buffer, Offset, Count, default).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> Dest)
        {
            var Temp = new byte[2048];
            while (Dest.Length > 0 && !m_Transport.Completion.IsCompleted)
            {
                var Slice = Math.Min(Temp.Length, Dest.Length);
                if (Slice <= 0)
                    break;

                Write(Temp, 0, Slice);

                Dest.Slice(0, Slice).CopyTo(new ArraySegment<byte>(Temp, 0, Slice));
                Dest = Dest.Slice(Slice, Dest.Length - Slice);
            }
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] Buffer, int Offset, int Count, CancellationToken Cancellation)
            => m_Transport.WriteAsync(new ArraySegment<byte>(Buffer, Offset, Count), Cancellation);

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> Dest, CancellationToken Cancellation = default)
        {
            var Temp = new byte[2048];
            while (Dest.Length > 0 && !m_Transport.Completion.IsCompleted)
            {
                var Slice = Math.Min(Temp.Length, Dest.Length);
                if (Slice <= 0)
                    break;

                Dest.Span.Slice(0, Slice).CopyTo(new ArraySegment<byte>(Temp, 0, Slice));
                Dest = Dest.Slice(Slice, Dest.Length - Slice);

                await WriteAsync(Temp, 0, Slice, Cancellation);
            }

            if (m_Transport.Completion.IsCompleted)
                throw new InvalidOperationException("The opacity stream has been closed.");
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback Callback, object State)
        {
            var Task = WriteAsync(buffer, offset, count);
            Task.ContinueWith(X => ThreadPool.QueueUserWorkItem(_ => Callback?.Invoke(Task)));

            return new TaskToAsyncResult
            {
                Task = Task,
                AsyncState = State
            };
        }

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult IAR)
        {
            if (!(IAR is TaskToAsyncResult Adapter))
                throw new InvalidOperationException("the async result isn't from the BeginRead method.");

            try { Adapter.Task.GetAwaiter().GetResult(); }
            catch
            {
            }
        }

        /// <inheritdoc/>
        public override void Flush() {  }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override ValueTask DisposeAsync() => m_Transport.DisposeAsync();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
            => DisposeAsync().GetAwaiter().GetResult();

    }
}
