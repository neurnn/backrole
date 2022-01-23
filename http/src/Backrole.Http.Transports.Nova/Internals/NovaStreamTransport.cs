using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals
{
    internal class NovaStreamTransport : INovaStreamTransport
    {
        private TaskCompletionSource m_Tcs = new();
        private TcpClient m_Tcp;
        private Socket m_Socket;

        private ArraySegment<byte> m_AbsoluteBuffer;
        private List<ArraySegment<byte>> m_Buffers = new();

        private List<Task> m_Tasks = new(); // -> all socket based operations.
        private Task m_TaskWrite = null;    // -> a latest writing operation.

        /// <summary>
        /// Initialize a new <see cref="NovaStreamTransport"/> instance.
        /// </summary>
        /// <param name="Tcp"></param>
        public NovaStreamTransport(TcpClient Tcp)
        {
            if (!(m_Tcp = Tcp).Connected)
                  m_Tcs.TrySetResult();

            LocalAddress = m_Tcp.Client.LocalEndPoint.ToString();
            RemoteAddress = m_Tcp.Client.RemoteEndPoint.ToString();

            m_AbsoluteBuffer = new byte[Math.Max(m_Tcp.ReceiveBufferSize, 2048)];
            m_Socket = m_Tcp.Client;
        }

        /// <inheritdoc/>
        public Task Completion => m_Tcs.Task;

        /// <inheritdoc/>
        public string LocalAddress { get; }

        /// <inheritdoc/>
        public string RemoteAddress { get; }

        /// <summary>
        /// Invoke the delegate with task registration.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="Fn"></param>
        /// <returns></returns>
        private async Task<TReturn> WithRegistration<TReturn>(Func<Task<TReturn>> Fn)
        {
            var Tcs = new TaskCompletionSource();
            lock (this)
                m_Tasks.Add(Tcs.Task);

            try { return await Fn(); }
            finally
            {
                lock (this)
                    m_Tasks.Remove(Tcs.Task);

                Tcs.TrySetResult();
            }
        }

        /// <summary>
        /// Wait the registered tasks are completed.
        /// </summary>
        /// <returns></returns>
        private async Task WaitRegistrations()
        {
            while (true)
            {
                Task Waits;

                lock(this)
                {
                    if (m_Tasks.Count <= 0)
                        break;

                    Waits = Task.WhenAny(m_Tasks.ToArray());
                }

                await Waits;
            }
        }

        /// <inheritdoc/>
        public Task<int> PeekAsync(ArraySegment<byte> Buffer, CancellationToken Cancellation = default)
        {
            return WithRegistration(async () =>
            {
                while (true)
                {
                    if (Completion.IsCompleted)
                    {
                        lock (m_Buffers)
                            m_Buffers.Clear();

                        return 0;
                    }

                    lock (m_Buffers)
                    {
                        if (m_Buffers.Count > 0)
                            return TryPeekOnce(ref Buffer);
                    }

                    int Length = 0;

                    try { Length = await m_Socket.ReceiveAsync(m_AbsoluteBuffer, SocketFlags.None, Cancellation); }
                    catch (Exception Exception)
                    {
                        if (Exception is SocketException Socket)
                        {
                            if (CheckIsNotError(Socket))
                                continue;
                        }

                        Cancellation.ThrowIfCancellationRequested();
                    }

                    if (Length <= 0)
                    {
                        Cleanup();
                        continue;
                    }

                    if (Length != m_AbsoluteBuffer.Count)
                    {
                        var New = new byte[Length];
                        Array.Copy(m_AbsoluteBuffer.Array, m_AbsoluteBuffer.Offset, New, 0, Length);
                        m_Buffers.Add(New);
                    }

                    else
                    {
                        m_Buffers.Add(m_AbsoluteBuffer);
                        m_AbsoluteBuffer = new byte[Math.Max(m_Tcp.ReceiveBufferSize, 2048)];
                    }
                }
            });
        }

        /// <inheritdoc/>
        public Task<int> BufferAsync(bool Enforce = false, CancellationToken Cancellation = default)
        {
            return WithRegistration(async () =>
            {
                while (true)
                {
                    if (Completion.IsCompleted)
                    {
                        lock (m_Buffers)
                            m_Buffers.Clear();

                        return 0;
                    }

                    lock (m_Buffers)
                    {
                        if (m_Buffers.Count > 0 && !Enforce)
                            return m_Buffers.Sum(X => X.Count);
                    }

                    int Length = 0;

                    try { Length = await m_Socket.ReceiveAsync(m_AbsoluteBuffer, SocketFlags.None, Cancellation); }
                    catch (Exception Exception)
                    {
                        if (Exception is SocketException Socket)
                        {
                            if (CheckIsNotError(Socket))
                                continue;
                        }

                        Cancellation.ThrowIfCancellationRequested();
                    }

                    if (Length <= 0)
                    {
                        Cleanup();
                        continue;
                    }

                    if (Length != m_AbsoluteBuffer.Count)
                    {
                        var New = new byte[Length];
                        Array.Copy(m_AbsoluteBuffer.Array, m_AbsoluteBuffer.Offset, New, 0, Length);
                        m_Buffers.Add(New);
                    }

                    else
                    {
                        m_Buffers.Add(m_AbsoluteBuffer);
                        m_AbsoluteBuffer = new byte[Math.Max(m_Tcp.ReceiveBufferSize, 2048)];
                    }

                    Enforce = false;
                }
            });
        }

        /// <summary>
        /// Check the <see cref="SocketException"/> is an error or not.
        /// </summary>
        /// <param name="Socket"></param>
        /// <returns></returns>
        private static bool CheckIsNotError(SocketException Socket)
        {
            return Socket.SocketErrorCode == SocketError.Interrupted ||
                   Socket.SocketErrorCode == SocketError.WouldBlock ||
                   Socket.SocketErrorCode == SocketError.IOPending ||
                   Socket.SocketErrorCode == SocketError.InProgress;
        }

        /// <summary>
        /// Try Peek bytes from the buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        private int TryPeekOnce(ref ArraySegment<byte> Buffer)
        {
            var Index = 0;
            var Length = 0;

            while (Buffer.Count > 0)
            {
                if (m_Buffers.Count <= Index)
                    break;

                var Temp = m_Buffers[Index++];
                var TempLength = Math.Min(Buffer.Count, Temp.Count);

                Array.Copy(Temp.Array, Temp.Offset, Buffer.Array, Buffer.Offset, TempLength);
                Buffer = new ArraySegment<byte>(Buffer.Array, Buffer.Offset + TempLength, Buffer.Count - TempLength);

                Length += TempLength;
            }

            return Length;
        }

        /// <inheritdoc/>
        public int AdvanceTo(int Length)
        {
            var TotalLength = 0;
            lock (this)
            {
                var Index = 0;
                while (Length > 0)
                {
                    if (m_Buffers.Count <= Index)
                        break;

                    var Current = m_Buffers[Index];
                    var Slice = Math.Min(Current.Count, Length);

                    if (Current.Count == Slice)
                        m_Buffers.RemoveAt(Index--);

                    else
                    {
                        m_Buffers[Index] = new ArraySegment<byte>(
                            Current.Array, Current.Offset + Slice, Current.Count - Slice);
                    }

                    TotalLength += Slice;
                    Length -= Slice;

                    Index++;
                }
            }

            return TotalLength;
        }

        /// <inheritdoc/>
        public async Task<int> ReadAsync(ArraySegment<byte> Buffer, CancellationToken Cancellation = default)
        {
            var Length = await PeekAsync(Buffer, Cancellation);
            if (Length > 0)
                return AdvanceTo(Length);

            return 0;
        }

        /// <inheritdoc/>
        public Task WriteAsync(ArraySegment<byte> Buffer, CancellationToken Cancellation = default)
        {
            return WithRegistration(async () =>
            {
                var Tcs = new TaskCompletionSource();
                while (true)
                {
                    Task Waits;
                    lock (this)
                    {
                        if (m_TaskWrite is null || m_TaskWrite.IsCompleted)
                        {
                            m_TaskWrite = Tcs.Task;
                            break;
                        }

                        Waits = m_TaskWrite;
                    }

                    await Waits;
                }

                try
                {
                    while (Buffer.Count > 0)
                    {
                        if (Completion.IsCompleted)
                        {
                            lock (m_Buffers)
                                m_Buffers.Clear();

                            break;
                        }

                        int Length = 0;
                        try { Length = await m_Socket.SendAsync(Buffer, SocketFlags.None, Cancellation); }
                        catch (Exception Exception)
                        {
                            if (Exception is SocketException Socket)
                            {
                                if (CheckIsNotError(Socket))
                                    continue;
                            }

                            Cancellation.ThrowIfCancellationRequested();
                        }

                        if (Length <= 0)
                        {
                            Cleanup();
                            continue;
                        }

                        Buffer = new ArraySegment<byte>(Buffer.Array,
                            Buffer.Offset + Length, Buffer.Count - Length);
                    }
                }

                finally { Tcs.TrySetResult(); }
                return 0;
            });
        }

        /// <summary>
        /// Cleanup the socket.
        /// </summary>
        private void Cleanup()
        {
            if (m_Tcs.TrySetResult())
            {
                try { m_Tcp.Close(); }
                catch { }

                try { m_Tcp.Dispose(); }
                catch { }
            }
        }

        /// <inheritdoc/>
        public Task CloseAsync()
        {
            Cleanup();

            return WaitRegistrations();
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => new ValueTask(CloseAsync());
    }
}
