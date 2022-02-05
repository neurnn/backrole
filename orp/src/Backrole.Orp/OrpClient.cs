using Backrole.Orp.Abstractions;
using Backrole.Orp.Internals;
using Backrole.Orp.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backrole.Orp
{
    public class OrpClient : IOrpClient
    {
        private static readonly object[] EMPTY_ARGS = new object[0];

        private Task m_Task;
        private Channel<OrpMessage> m_Incomings;
        private OutgoingManager m_Outgoings;
        private Action m_Terminate;

        private bool m_SharedQueue = false;

        /// <summary>
        /// Initialize a new <see cref="OrpClient"/> instance.
        /// </summary>
        /// <param name="Options"></param>
        /// <param name="Tcp"></param>
        public OrpClient(IOrpReadOnlyOptions Options)
        {
            IsServerMode = false;
            this.Options = Options;
        }

        /// <summary>
        /// Initialize a new <see cref="OrpClient"/> instance as Server-Mode.
        /// </summary>
        /// <param name="Options"></param>
        /// <param name="Tcp"></param>
        internal OrpClient(IOrpReadOnlyOptions Options, TcpClient Tcp, Channel<OrpMessage> Incomings, CancellationToken Token)
        {
            IsServerMode = true;
            RemoteEndPoint = Tcp.Client.RemoteEndPoint as IPEndPoint;
            this.Options = Options;

            PrepareChannels(Tcp, Incomings);
            m_Terminate = () => Quietly(Tcp.Client.Close);
            m_Task = RunAsync(Tcp, Token);
        }

        /// <summary>
        /// Prepare the channels.
        /// </summary>
        private void PrepareChannels(TcpClient Tcp, Channel<OrpMessage> Incomings = null)
        {
            CleanupChannels();

            m_SharedQueue = Incomings != null;
            m_Incomings = Incomings ?? Channel.CreateBounded<OrpMessage>(Options.IncomingQueueSize);
            m_Outgoings = new OutgoingManager(Tcp, this, Options);
        }

        /// <summary>
        /// Cleanup all channels.
        /// </summary>
        private void CleanupChannels()
        {
            if (m_SharedQueue)
                m_Incomings = null;

            m_Incomings?.Writer.TryComplete();
            m_Outgoings?.Dispose();
            m_SharedQueue = false;
        }

        /// <inheritdoc/>
        public bool IsConnected => m_Task != null && !m_Task.IsCompleted;

        /// <inheritdoc/>
        public bool IsServerMode { get; private set; } = false;

        /// <inheritdoc/>
        public IOrpReadOnlyOptions Options { get; }

        /// <inheritdoc/>
        public IPEndPoint RemoteEndPoint { get; private set; }

        /// <inheritdoc/>
        public object UserState { get; set; }

        /// <inheritdoc/>
        public event Action<IOrpClient> Connected;

        /// <inheritdoc/>
        public event Action<IOrpClient> Disconnected;

        /// <summary>
        /// Reserve an action that invoked after the connection closed.
        /// </summary>
        /// <param name="Behaviour"></param>
        internal void OnDisconnected(Action<OrpClient> Behaviour)
        {
            if (IsConnected)
            {
                m_Task.ContinueWith(_ => Behaviour(this));
                return;
            }

            Task.Run(() => Behaviour(this));
        }

        /// <summary>
        /// Invoke an action quietly.
        /// </summary>
        /// <param name="Action"></param>
        private static void Quietly(Action Action)
        {
            try { Action?.Invoke(); }
            catch { }
        }

        /// <summary>
        /// Run the communication loop.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        private async Task RunAsync(TcpClient Tcp, CancellationToken Token)
        {
            var Receiver = null as Task<byte[]>;
            using (Token.Register(() => Quietly(Tcp.Client.Close)))
            {
                var TaskNotify = Task.Run(() =>
                {
                    if (!IsServerMode)
                        Connected?.Invoke(this);
                });

                try
                {
                    while (true)
                    {
                        byte[] Packet;
                        try
                        {
                            if ((Packet = await Tcp.ReceiveChunkedAsync()) is null)
                                break;
                        }

                        catch { break; }
                        await OnPacket(Tcp, Packet);
                    }
                }

                finally
                {
                    Quietly(Tcp.Client.Close);
                    Quietly(Tcp.Dispose);
                    CleanupChannels();

                    await TaskNotify;
                    if (!IsServerMode)
                        Disconnected?.Invoke(this);

                    IsServerMode = false;
                }
            }
        }

        /// <summary>
        /// Called when the packet bytes received.
        /// </summary>
        /// <param name="Tcp"></param>
        /// <param name="Packet"></param>
        /// <returns></returns>
        private async Task<bool> OnPacket(TcpClient Tcp, byte[] Packet)
        {
            using var Stream = new MemoryStream(Packet, false);
            using var Reader = new EndianessReader(Stream, null, true, Options.UseLittleEndian);

            var Opcode = Reader.ReadByte();
            if (Opcode == 0x10) // Cmd.
            {
                byte[] EmitId;
                Type Type;

                try
                {
                    EmitId = Reader.ReadBytes(16);
                    var Name = Reader.ReadString();

                    if (!Options.TryGetType(Name, out Type))
                        return await EmitFeedbackAsync(Tcp, EmitId, false);
                }
                catch { return false; }

                var Message = UnpackMessage(Reader, Type, Options);
                if (Message != null)
                {
                    try { await m_Incomings.Writer.WriteAsync(new OrpMessage(this, DateTime.UtcNow, Message)); }
                    catch { Message = null; }
                }

                return await EmitFeedbackAsync(Tcp, EmitId, Message != null);
            }

            else if (Opcode == 0x11) // Ack.
            {
                var EmitId = Reader.ReadBytes(16);
                var Result = Reader.ReadByte() != 0x00;
                m_Outgoings.SetEmitAck(new Guid(EmitId), Result);
            }

            return true;
        }

        /// <summary>
        /// Unpack the message object from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="Reader"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        internal static object UnpackMessage(BinaryReader Reader, Type Type, IOrpReadOnlyOptions Options)
        {
            var Ctor = Type.GetConstructor(Type.EmptyTypes);
            if (Ctor is null) return null;

            object Message;
            if (Debugger.IsAttached)
            {
                if ((Message = Ctor.Invoke(EMPTY_ARGS)) is not IOrpUnpackable Unpacker)
                    return null;

                if (!Unpacker.TryUnpack(Reader, Options))
                    return null;

                return Message;
            }

            try
            {
                if ((Message = Ctor.Invoke(EMPTY_ARGS)) is not IOrpUnpackable Unpacker)
                    return null;

                if (!Unpacker.TryUnpack(Reader, Options))
                    return null;

                return Message;
            }

            catch { }
            return null;
        }

        /// <inheritdoc/>
        private Task<bool> EmitFeedbackAsync(TcpClient Tcp, byte[] Guid, bool Result)
        {
            using var Stream = new MemoryStream();
            using (var Writer = new EndianessWriter(Stream, null, true, Options.UseLittleEndian))
            {
                Writer.Write((byte)0x11);
                Writer.Write(Guid);
                Writer.Write((byte)(Result ? 0x01 : 0x00));
            }

            return Tcp.EmitChunkedAsync(Stream.ToArray());
        }

        /// <inheritdoc/>
        public Task<bool> ConnectAsync(IPEndPoint RemoteEP, CancellationToken Token = default)
            => ConnectAsync(RemoteEP, null, Token);

        /// <summary>
        /// Connect to remote host.
        /// </summary>
        /// <param name="RemoteEP"></param>
        /// <param name="Queue"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        internal async Task<bool> ConnectAsync(IPEndPoint RemoteEP, Channel<OrpMessage> Queue, CancellationToken Token = default)
        {
            if (IsConnected)
                throw new InvalidOperationException("The connection is alive yet. to reuse this instance, disconnect first.");

            while (!Token.IsCancellationRequested)
            {
                var Tcp = new TcpClient();

                try { await Tcp.ConnectAsync(RemoteEP.Address, RemoteEP.Port, Token); }
                catch
                {
                    if (Tcp.Client != null)
                        Quietly(Tcp.Client.Close);

                    Quietly(Tcp.Dispose);
                    continue;
                }

                IsServerMode = false;
                RemoteEndPoint = RemoteEP;

                PrepareChannels(Tcp, Queue);
                m_Terminate = () => Quietly(Tcp.Client.Close);
                m_Task = RunAsync(Tcp, Token);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<OrpMessage> WaitAsync(CancellationToken Token = default)
        {
            if (IsConnected)
            {
                if (IsServerMode)
                    throw new NotSupportedException("Server-Mode client can not wait message by single instance.");

                if (m_SharedQueue)
                    throw new NotSupportedException("Clients who uses the shared queue can not wait message by single instance.");

                try { return await m_Incomings.Reader.ReadAsync(Token).AsTask(); }
                catch
                {
                }

                Token.ThrowIfCancellationRequested();
            }

            throw new InvalidOperationException("Connection is dead.");
        }

        /// <inheritdoc/>
        public async Task<OrpEmitStatus> EmitAsync(object Message, CancellationToken Token = default)
        {
            if (Message is null)
                throw new ArgumentNullException(nameof(Message));

            if (Message is not IOrpPackable Packer || !Options.TryGetName(Message.GetType(), out var Name))
                throw new ArgumentException("Not mapped type, or it should implement IOrpPackable to be emitted.");

            if (IsConnected)
                return await m_Outgoings.SendAsync(Name, Packer, Token);

            throw new InvalidOperationException("Connection is dead.");
        }

        /// <inheritdoc/>
        public ValueTask DisconnectAsync()
        {
            if (IsConnected)
            {
                m_Terminate?.Invoke();
                m_Terminate = null;

                return new ValueTask(m_Task);
            }

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => DisconnectAsync();
    }
}
