using Backrole.Orp.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backrole.Orp
{
    public class OrpServer : IOrpServer
    {
        private Task m_TaskListen;
        private TaskCompletionSource m_TcsStop;
        private Channel<OrpMessage> m_Incomings;
        private bool m_SharedIncomings = false;

        private HashSet<OrpClient> m_Connections = new();

        /// <summary>
        /// Initialize a new <see cref="OrpServer"/> instance.
        /// </summary>
        /// <param name="LocalEP"></param>
        public OrpServer(IOrpReadOnlyOptions Options, IPEndPoint LocalEP)
        {
            LocalEndPoint = LocalEP;
            this.Options = Options;
        }

        /// <inheritdoc/>
        public bool IsListening => m_TaskListen != null && !m_TaskListen.IsCompleted;

        /// <inheritdoc/>
        public IPEndPoint LocalEndPoint { get; }

        /// <inheritdoc/>
        public IOrpReadOnlyOptions Options { get; }

        /// <inheritdoc/>
        public event Action<IOrpServer, IOrpClient> Connected;

        /// <inheritdoc/>
        public event Action<IOrpServer, IOrpClient> Disconnected;

        /// <inheritdoc/>
        public bool Start(int Backlog = 64) => Start(null, Backlog);

        /// <inheritdoc/>
        internal bool Start(Channel<OrpMessage> Queue, int Backlog = 64)
        {
            lock (this)
            {
                if (IsListening)
                    return false;

                m_TcsStop = new TaskCompletionSource();
                m_Incomings = Queue ?? Channel.CreateBounded<OrpMessage>(Options.IncomingQueueSize);
                m_SharedIncomings = Queue != null;
                m_TaskListen = ListenAsync(m_TcsStop.Task, Backlog);
                return true;
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (IsListening)
            {
                m_TcsStop?.TrySetResult();
                if (!m_SharedIncomings)
                    m_Incomings.Writer.TryComplete();

                m_TaskListen.GetAwaiter().GetResult();
            }
        }

        /// <inheritdoc/>
        private async Task ListenAsync(Task Trigger, int Backlog)
        {
            using var Cts = new CancellationTokenSource(); // To propagate the termination event to connection tasks.
            var Listener = new TcpListener(LocalEndPoint);
            var Accepter = null as Task<TcpClient>;

            Listener.Start(Backlog);

            try
            {
                while (!Trigger.IsCompleted)
                {
                    if (Accepter is null)
                        Accepter = Listener.AcceptTcpClientAsync();

                    if (Accepter.IsCompleted)
                    {
                        var Newbie = new OrpClient(Options, await Accepter, m_Incomings, Cts.Token);
                        lock (m_Connections)
                            m_Connections.Add(Newbie);

                        Accepter = null;
                        Connected?.Invoke(this, Newbie);
                        // Upgrade to ORP Client.

                        Newbie.OnDisconnected(OnDisconnect);
                        continue;
                    }

                    await Task.WhenAny(Accepter, Trigger);
                }
            }

            finally
            {
                lock (m_Connections)
                    m_Connections.Clear();

                if (!Cts.IsCancellationRequested)
                     Cts.Cancel();

                try { Listener.Stop(); }
                catch { }

                try
                {
                    if (Accepter != null)
                    {
                        var Newbie = await Accepter;
                        if (Newbie != null)
                            Newbie.Dispose();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Raise the disconnected event and remove the connection from the hashset.
        /// </summary>
        /// <param name="Connection"></param>
        private void OnDisconnect(OrpClient Connection)
        {
            lock (m_Connections)
            {
                if (!m_Connections.Remove(Connection))
                    return;
            }

            Disconnected?.Invoke(this, Connection);
        }

        /// <inheritdoc/>
        public async Task<OrpMessage> WaitAsync(CancellationToken Token = default)
        {
            if (IsListening && m_Incomings != null)
            {
                if (m_SharedIncomings)
                    throw new InvalidOperationException("The server that uses shared queue can not wait message using this method.");

                try { return await m_Incomings.Reader.ReadAsync(Token); }
                catch
                {
                    Token.ThrowIfCancellationRequested();
                }
            }

            throw new InvalidOperationException("the server isn't started yet.");
        }

        /// <inheritdoc/>
        public async Task<OrpBroadcastStatus> BroadcastAsync(object Message, CancellationToken Token = default)
        {
            if (IsListening)
            {
                OrpClient[] Connections;
                lock (m_Connections)
                    Connections = m_Connections.ToArray();

                var ReallySent = new List<IOrpClient>();
                var TimeStamp = DateTime.UtcNow;
                foreach(var Each in Connections.Where(X => X.IsConnected))
                {
                    if (Token.IsCancellationRequested)
                        break;

                    try
                    {
                        await Each.EmitAsync(Message, Token);
                        ReallySent.Add(Each);
                    }
                    catch { }
                }

                if (ReallySent.Count <= 0)
                    Token.ThrowIfCancellationRequested();

                return new OrpBroadcastStatus(ReallySent.ToArray(), TimeStamp, Message);
            }

            throw new InvalidOperationException("the server isn't started yet.");
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if (IsListening)
            {
                m_TcsStop?.TrySetResult();
                return new ValueTask(m_TaskListen);
            }

            return ValueTask.CompletedTask;
        }

    }
}
