using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Http1.Outputs;
using Backrole.Http.Transports.Nova.Internals.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1
{
    internal class NovaHttpOutputStream : Stream
    {
        private static readonly byte[] EMPTY_BYTES = new byte[0];

        private INovaStreamTransport m_Transport;
        private NovaHttpResponse m_Response;
        private BaseOutputs m_Outputs;

        private Task m_Initiate;
        private Stream m_DataStream;

        /// <summary>
        /// Initialize a new <see cref="NovaHttpOutputStream"/> instance.
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Response"></param>
        public NovaHttpOutputStream(INovaStreamTransport Transport, NovaHttpResponse Response)
        {
            m_Transport = Transport;
            m_Response = Response;
        }

        /// <summary>
        /// Initiate the output stream and this will write HTTP headers.
        /// </summary>
        /// <returns></returns>
        private Task InitiateAsync(bool FromDispose = false)
        {
            async Task RunAsync()
            {
                if (!m_Response.TrySetSent())
                    return;

                var Stream = m_Response.OutputStream;
                if (Stream is null || (FromDispose && Stream is NovaHttpOutputStream))
                    Stream = new MemoryStream(EMPTY_BYTES, false);

                if (Stream != null && !(Stream is NovaHttpOutputStream) && Stream.CanSeek)
                {
                    m_Response.Headers.Remove("Transfer-Encoding");
                    m_Response.Headers.Set("Content-Length", Stream.Length.ToString());
                }

                m_DataStream = Stream;
                m_Outputs = BaseOutputs.Create(m_Transport, m_Response);

                await WriteHeaderBytes();
            }

            lock(this)
            {
                if (m_Initiate is null)
                    m_Initiate = RunAsync();

                return m_Initiate;
            }
        }

        /// <summary>
        /// Send the header bytes.
        /// </summary>
        /// <returns></returns>
        private async Task WriteHeaderBytes()
        {
            var ProtocolVersion = m_Response.Context.Request.Protocol;
            if (m_Response.StatusPhrase is null)
            {
                NovaHttpStatusCodes.Table.TryGetValue(m_Response.Status, out var Phrase);
                m_Response.StatusPhrase = Phrase ?? "Unknown";
            }

            if (!string.IsNullOrWhiteSpace(m_Response.Headers.GetValue("Upgrade")))
                m_Response.Headers.Remove("Content-Length").Remove("Transfer-Encoding");

            var VSP = $"{ProtocolVersion} {m_Response.Status} {m_Response.StatusPhrase}";
            var HKV = string.Join("\r\n", m_Response.Headers.OrderBy(X => X.Key)
                .Select(X => $"{X.Key}: {X.Value}".Trim()).Where(X => X.Length > 1));

            var HeaderBytes = Encoding.ASCII.GetBytes(string.Join("\r\n", VSP, HKV, "\r\n"));
            await m_Transport.WriteAsync(HeaderBytes);
        }

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

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
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
            => WriteAsync(buffer, offset, count, default).GetAwaiter().GetResult();

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
        public override async Task WriteAsync(byte[] Buffer, int Offset, int Count, CancellationToken Cancellation)
        {
            await InitiateAsync();

            if (!m_Transport.Completion.IsCompleted)
                await m_Outputs.WriteAsync(new ArraySegment<byte>(Buffer, Offset, Count), Cancellation);
        }

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
        public override void Close()
        {
            DisposeAsync().GetAwaiter().GetResult();
            base.Close();
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            await InitiateAsync(true);
            try
            {
                if (m_DataStream != null && !(m_DataStream is NovaHttpOutputStream))
                {
                    try
                    {
                        await m_DataStream.CopyToAsync(this);
                    }

                    finally
                    {
                        await m_DataStream.DisposeAsync();
                    }
                }
            }
            finally
            {
                if (m_Outputs != null)
                    await m_Outputs.DisposeAsync();
            }
        }
    }
}
