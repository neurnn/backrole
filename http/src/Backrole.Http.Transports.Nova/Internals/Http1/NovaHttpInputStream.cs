using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Http1.Inputs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1
{
    internal partial class NovaHttpInputStream : Stream
    {
        private BaseInputs m_Inputs;

        /// <summary>
        /// Initialize a new <see cref="NovaHttpInputStream"/> instance.
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Request"></param>
        public NovaHttpInputStream(INovaStreamTransport Transport, IHttpRequest Request) 
            => m_Inputs = BaseInputs.Create(Transport, Request);

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) 
            => ReadAsync(buffer, offset, count, default).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override int Read(Span<byte> Dest)
        {
            var Temp = new byte[2048];
            var TotalLength = 0;

            while (Dest.Length > 0)
            {
                var Slice = Math.Min(Temp.Length, Dest.Length);
                if (Slice <= 0)
                    break;

                if ((Slice = Read(Temp, 0, Slice)) <= 0)
                    break;

                new ArraySegment<byte>(Temp, 0, Slice)
                    .AsSpan().CopyTo(Dest.Slice(0, Slice));

                Dest = Dest.Slice(Slice, Dest.Length - Slice);
                TotalLength += Slice;
            }

            return TotalLength;
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] Buffer, int Offset, int Count, CancellationToken Cancellation = default) 
            => m_Inputs.ReadAsync(new ArraySegment<byte>(Buffer, Offset, Count), Cancellation);

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
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();
        
        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void WriteByte(byte value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Close()
        {
            DisposeAsync().GetAwaiter().GetResult();
            base.Close();
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync() => m_Inputs.DisposeAsync();
    }
}
