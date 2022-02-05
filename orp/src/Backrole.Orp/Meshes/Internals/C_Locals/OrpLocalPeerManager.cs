using Backrole.Orp.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backrole.Orp.Meshes.Internals.C_Locals
{
    internal class OrpLocalPeerManager : IAsyncDisposable
    {
        private List<OrpLocalPeer> m_Peers = new();
        private TaskCompletionSource m_Empty = new();
        private Task m_Worker;

        private CancellationTokenSource m_Cts;
        private OrpMesh m_Mesh;

        private Channel<OrpMessage> m_Incomings;
        private bool m_Disposed = false;

        /// <summary>
        /// Initialize a new <see cref="OrpLocalPeerManager"/> instance.
        /// </summary>
        /// <param name="Server"></param>
        public OrpLocalPeerManager(OrpMesh Mesh)
        {
            m_Worker = Task.CompletedTask;
            m_Incomings = Channel.CreateBounded<OrpMessage>(Mesh.Options.ProtocolOptions.IncomingQueueSize);
            m_Mesh = Mesh;
        }

        /// <summary>
        /// Test whether the peer is still here or not.
        /// </summary>
        /// <param name="Peer"></param>
        /// <returns></returns>
        public bool ContainsPeer(OrpLocalPeer Peer)
        {
            lock(m_Peers)
            {
                return m_Peers.Contains(Peer);
            }
        }

        /// <summary>
        /// Add a local peer.
        /// </summary>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public bool AddPeer(IPEndPoint EndPoint)
        {
            lock(m_Peers)
            {
                if (m_Disposed)
                    return false;

                if (m_Peers.Find(X => X.RemoteEndPoint == EndPoint) != null)
                    return false;

                var Peer = new OrpLocalPeer(this, EndPoint, m_Mesh);

                Peer.Connection.UserState = Peer;
                Peer.Connection.Disconnected += OnDisconnected;

                m_Peers.Add(Peer);
                m_Empty.TrySetResult();

                Peer.SetState(OrpMeshPeerState.Pending);
                return true;
            }
        }

        /// <summary>
        /// Remove a local peer.
        /// </summary>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public bool RemovePeer(IPEndPoint EndPoint)
        {
            OrpLocalPeer Peer;
            lock (m_Peers)
            {
                Peer = m_Peers.Find(X => X.RemoteEndPoint == EndPoint);
            }

            if (Peer != null)
                return RemovePeer(Peer);

            return false;
        }

        /// <summary>
        /// Remove a local peer.
        /// </summary>
        /// <param name="Peer"></param>
        /// <returns></returns>
        public bool RemovePeer(OrpLocalPeer Peer)
        {
            lock(m_Peers)
            {
                if (!m_Peers.Remove(Peer))
                    return false;

                if (m_Peers.Count <= 0 && m_Empty.Task.IsCompleted)
                {
                    m_Empty = new TaskCompletionSource();
                    m_Peers.TrimExcess();
                }
            }

            try
            {
                Peer.Connection.Dispose();
                Peer.SetState(OrpMeshPeerState.Removed);
            }

            finally
            {
                Peer.Connection.Disconnected -= OnDisconnected;
            }

            return true;
        }

        /// <summary>
        /// Called when the connection has been kicked.
        /// </summary>
        /// <param name="Connection"></param>
        private void OnDisconnected(IOrpClient Connection)
        {
            if (Connection.UserState is not OrpLocalPeer Peer)
                return;

            Peer.OnDisconnected();
            Peer.SetState(OrpMeshPeerState.Disconnected);
        }

        /// <summary>
        /// Run the worker that handles remote peers.
        /// </summary>
        public void RunWorker()
        {
            lock(this)
            {
                if (m_Worker.IsCompleted)
                {
                    m_Disposed = false;

                    if (m_Cts != null)
                        m_Cts.Dispose();

                    m_Cts = new CancellationTokenSource();
                    m_Worker = RunAsync(m_Cts.Token);
                }
            }
        }

        /// <summary>
        /// Get Worker task.
        /// </summary>
        /// <returns></returns>
        public Task GetWorker() => m_Worker;

        /// <summary>
        /// Event that notifies about the peer status changed.
        /// </summary>
        public event Action<OrpLocalPeerManager, OrpLocalPeer> StatusChanged;

        /// <summary>
        /// Notify the <see cref="StatusChanged"/> event.
        /// </summary>
        /// <param name="Peer"></param>
        public void Notify(OrpLocalPeer Peer) => StatusChanged?.Invoke(this, Peer);

        /// <summary>
        /// Wait the peers are not empty.
        /// </summary>
        /// <param name="Peers"></param>
        /// <returns></returns>
        private Task WaitNotEmpty(Queue<OrpLocalPeer> Peers)
        {
            lock (m_Peers)
            {
                foreach (var Each in m_Peers)
                    Peers.Enqueue(Each);

                Peers.TrimExcess();
                return m_Empty.Task;
            }
        }

        /// <summary>
        /// Run the <see cref="OrpLocalPeerManager"/> instance.
        /// </summary>
        /// <param name="Server"></param>
        /// <returns></returns>
        private async Task RunAsync(CancellationToken Token)
        {
            var Queue = new Queue<OrpLocalPeer>();
            var Router = RouteAsync();

            while (!Token.IsCancellationRequested)
            {
                var Now = DateTime.Now;
                await WaitNotEmpty(Queue);

                while (Queue.TryDequeue(out var Each))
                {
                    switch (Each.State)
                    {
                        case OrpMeshPeerState.Connecting:
                        case OrpMeshPeerState.Connected:
                        case OrpMeshPeerState.Handshaking:
                            continue;

                        default:
                            break;
                    }

                    await Each.TickAsync(m_Incomings);
                }

                /* Make the time frame to be 10 ms. */
                var Spent = Math.Ceiling((DateTime.Now - Now).TotalMilliseconds);
                var Delay = (int)Math.Max(0, 10 - Spent);
                if (Delay > 0)
                {
                    await Task.Delay(Delay);
                }
            }

            m_Incomings.Writer.TryComplete();
            await Router;
        }

        /// <summary>
        /// Route the incoming messages to peers.
        /// </summary>
        /// <returns></returns>
        private async Task RouteAsync()
        {
            while(true)
            {
                try
                {
                    if (!await m_Incomings.Reader.WaitToReadAsync())
                        break;
                }
                catch { break; }

                while (m_Incomings.Reader.TryRead(out var Message))
                {
                    if (Message.Source is null || Message.Message is null)
                        continue;

                    if (Message.Source.UserState is not OrpLocalPeer Peer)
                        continue;

                    if (Peer.State == OrpMeshPeerState.Connected)
                    {
                        var Modules = m_Mesh.Options.ProtocolModules;
                        var Queue = new Queue<IOrpMeshProtocolModule>(Modules);

                        await ExecuteModules(Peer, Queue, Message);
                        continue;
                    }

                    await Peer.HandleAsync(Message.Message);
                }
            }
        }

        [DebuggerHidden]
        private Task ExecuteModules(OrpLocalPeer Peer, Queue<IOrpMeshProtocolModule> Queue, OrpMessage Message)
        {
            if (Queue.TryDequeue(out var Module))
                return Module.OnMessageAsync(Message, () => ExecuteModules(Peer, Queue, Message));

            return Peer.HandleAsync(Message);
        }

        /// <summary>
        /// Get the current <see cref="CancellationTokenSource"/> instance.
        /// </summary>
        /// <returns></returns>
        private CancellationTokenSource GetCts()
        {
            lock(this)
            {
                var Cts = m_Cts;
                m_Cts = null;
                return Cts;
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            lock (this)
            {
                if (m_Disposed)
                    return;

                m_Disposed = true;
            }

            var Cts = GetCts();
            var Worker = GetWorker();

            while (true)
            {
                OrpLocalPeer Peer;

                lock (m_Peers)
                {
                    if ((Peer = m_Peers.LastOrDefault()) is null)
                        break;
                }

                RemovePeer(Peer);
            }

            Cts?.Cancel();

            m_Empty?.TrySetResult();

            if (Worker != null)
                await Worker;

            Cts?.Dispose();
        }
    }

}
