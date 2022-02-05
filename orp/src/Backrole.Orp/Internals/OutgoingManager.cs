using Backrole.Orp.Abstractions;
using Backrole.Orp.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Internals
{
    internal class OutgoingManager : IDisposable, IAsyncDisposable
    {
        private IOrpReadOnlyOptions m_Options;
        private IOrpClient m_Orp;
        private TcpClient m_Tcp;

        private Dictionary<Guid, TaskCompletionSource<bool>> m_EmitStats = new();
        private Task m_Lock;

        private bool m_Disposed = false;

        /// <summary>
        /// Initialize a new <see cref="OutgoingManager"/> instance.
        /// </summary>
        /// <param name="Tcp"></param>
        public OutgoingManager(TcpClient Tcp, IOrpClient Orp, IOrpReadOnlyOptions Options)
        {
            m_Tcp = Tcp;
            m_Orp = Orp;
            m_Options = Options;
        }

        /// <summary>
        /// Set Emit acknowledgement.
        /// </summary>
        /// <param name="Emitid"></param>
        /// <param name="Result"></param>
        public void SetEmitAck(Guid Emitid, bool Result)
        {
            TaskCompletionSource<bool> Tcs;
            lock (m_EmitStats)
            {
                if (!m_EmitStats.TryGetValue(Emitid, out Tcs))
                    return;
            }

            /*
             * TaskCompletionSource causes infinite blocking.
             * https://github.com/EventStore/EventStore/issues/1179
             * Issue: TaskCompletionSource.SetResult yields thread back to user land - and it can cause deadlocks.
             * 
             * Temporary Solution is ....... (not graceful... need to be reviewed)
             */
            ThreadPool.QueueUserWorkItem(_ => Tcs?.TrySetResult(Result));
        }

        /// <summary>
        /// Send the message to the remote host.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Packer"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public async Task<OrpEmitStatus> SendAsync(string Name, IOrpPackable Packer, CancellationToken Token = default)
        {
            var EmitId = MakeEmitId(out var Ack);
            try
            {
                using var Stream = new MemoryStream();
                using (var Writer = new EndianessWriter(Stream, null, true, m_Options.UseLittleEndian))
                {
                    Writer.Write((byte)0x10);
                    Writer.Write(EmitId.ToByteArray());
                    Writer.Write(Name);

                    if (!PackMessage(Writer, Packer))
                        throw new FormatException("Failed to pack the message");
                }

                var Tcs = new TaskCompletionSource<bool>();
                var TimeStamp = DateTime.UtcNow;

                using (Token.Register(() => Tcs.TrySetResult(false)))
                {
                    try
                    {
                        await WaitAsync(Tcs);
                        Token.ThrowIfCancellationRequested();

                        lock (this)
                        {
                            if (m_Disposed)
                                throw new InvalidOperationException("Connection is dead.");
                        }

                        /* only cancellable in here. */
                        if (!await m_Tcp.EmitChunkedAsync(Stream.ToArray(), Token))
                            throw new InvalidOperationException("Connection is dead.");
                    }
                    finally
                    {
                        /* 
                         * Release the lock here.
                         * */
                        Tcs.TrySetResult(true);
                    }
                }

                try { await Ack.Task; /* --> To wait the acknowledgement. */ }
                catch { throw new InvalidOperationException("Connection is dead."); }
                return new OrpEmitStatus(m_Orp, TimeStamp, Packer);
            }
            finally
            {
                RemoveEmitId(EmitId);
            }
        }

        /// <summary>
        /// Make Emit ID.
        /// </summary>
        /// <param name="Tcs"></param>
        /// <returns></returns>
        private Guid MakeEmitId(out TaskCompletionSource<bool> Tcs)
        {
            while (true)
            {
                var EmitId = Guid.NewGuid();

                lock (m_EmitStats)
                {
                    if (m_EmitStats.TryGetValue(EmitId, out Tcs))
                        continue;

                    m_EmitStats[EmitId] = Tcs = new TaskCompletionSource<bool>();
                    return EmitId;
                }
            }
        }

        /// <summary>
        /// Remove the Emit ID.
        /// </summary>
        /// <param name="Id"></param>
        private void RemoveEmitId(Guid Id)
        {
            TaskCompletionSource<bool> Tcs;
            lock (m_EmitStats)
            {
                if (!m_EmitStats.Remove(Id, out Tcs))
                    return;
            }

            Tcs?.TrySetCanceled();
        }

        /// <summary>
        /// Wait the outgoing task flow using the given Tcs.
        /// </summary>
        /// <param name="Tcs"></param>
        /// <returns></returns>
        private async Task<bool> WaitAsync(TaskCompletionSource<bool> Tcs)
        {
            while (!Tcs.Task.IsCompleted)
            {
                Task TaskLock;
                lock (this)
                {
                    if (m_Lock is null || m_Lock.IsCompleted)
                    {
                        m_Lock = Tcs.Task;
                        return true;
                    }

                    TaskLock = m_Lock;
                }

                await Task.WhenAny(Tcs.Task, TaskLock);
            }

            return false;
        }

        /// <summary>
        /// Pack the given message.
        /// </summary>
        /// <param name="Packer"></param>
        /// <returns></returns>
        private bool PackMessage(BinaryWriter Writer, IOrpPackable Packer)
        {
            if (Debugger.IsAttached)
            {
                if (!Packer.TryPack(Writer, m_Options))
                    return false;
            }

            else
            {
                try
                {
                    if (!Packer.TryPack(Writer, m_Options))
                        return false;
                }
                catch { return false; }
            }

            return true;
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            var Tcs = new TaskCompletionSource<bool>();
            if (!await WaitAsync(Tcs))
                return;

            try
            {
                lock (this)
                    m_Disposed = true;

                while (true)
                {
                    Guid[] Keys;

                    lock (m_EmitStats)
                    {
                        if (m_EmitStats.Keys.Count <= 0)
                            break;

                        Keys = m_EmitStats.Keys.ToArray();
                    }

                    foreach (var Each in Keys)
                        RemoveEmitId(Each);
                }
            }

            finally { Tcs.TrySetResult(true); }
        }
    }
}
