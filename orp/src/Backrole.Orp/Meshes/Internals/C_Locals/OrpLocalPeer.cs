using Backrole.Orp.Abstractions;
using Backrole.Orp.Meshes.Internals.A_Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backrole.Orp.Meshes.Internals.C_Locals
{
    internal class OrpLocalPeer : IOrpMeshPeer
    {
        /// <summary>
        /// Initialize a new <see cref="OrpLocalPeer"/> instance.
        /// </summary>
        /// <param name="Manager"></param>
        /// <param name="EndPoint"></param>
        /// <param name="Orp"></param>
        public OrpLocalPeer(OrpLocalPeerManager Manager, IPEndPoint EndPoint, OrpMesh Orp)
        {
            Peers = Manager; Mesh = Orp;
            State = OrpMeshPeerState.None;
            RemoteEndPoint = EndPoint;
            Connection = new OrpClient(Orp.Options.ProtocolOptions);
        }

        /// <inheritdoc/>
        public bool IsLocalPeer => true;

        /// <summary>
        /// Options.
        /// </summary>
        public OrpMesh Mesh { get; }

        /// <summary>
        /// Peer Manager.
        /// </summary>
        public OrpLocalPeerManager Peers { get; }

        /// <summary>
        /// Orp Connection.
        /// </summary>
        public OrpClient Connection { get; }

        /// <inheritdoc/>
        public IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Advertisement.
        /// </summary>
        public IPEndPoint RemoteAdvertisement { get; internal set; }

        /// <inheritdoc/>
        public OrpMeshToken RemoteMeshToken { get; internal set; }

        /// <inheritdoc/>
        public OrpMeshPeerState State { get; internal set; }

        /// <inheritdoc/>
        public object UserState { get; set; }

        /// <summary>
        /// Set the state of the peer.
        /// </summary>
        /// <param name="NewState"></param>
        /// <returns></returns>
        public OrpLocalPeer SetState(OrpMeshPeerState NewState)
        {
            if (State != NewState)
            {
                State = NewState;
                Peers.Notify(this);
            }

            return this;
        }

        /// <inheritdoc/>
        public async Task<OrpMeshEmitStatus> EmitAsync(object Message, CancellationToken Token = default)
        {
            if (State != OrpMeshPeerState.Connected)
                throw new InvalidOperationException("Invalid state to send message.");

            var Emit = await Connection.EmitAsync(Message, Token);
            return new OrpMeshEmitStatus(this, Emit.TimeStamp, Emit.Message);
        }

        /// <summary>
        /// Handle messages.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public async Task HandleAsync(object Message)
        {
            switch (State)
            {
                case OrpMeshPeerState.Handshaking:

                    try { await HandshakeAsync(Message); }
                    catch
                    {
                        await Connection.DisconnectAsync();
                    }
                    break;

                case OrpMeshPeerState.Connected:
                    await Mesh.MessageBox.OnMessageAsync(this, Message);
                    break;
            }
        }

        // ------------------------------------------------------------ HANDSHAKE.

        private INIT_Hello m_RemoteHello;
        private INIT_Hello m_LocalHello;

        private INIT_Check m_RemoteCheck;
        private INIT_Check m_LocalCheck;

        private INIT_CheckReply m_RemoteCheckReply;
        private INIT_CheckReply m_LocalCheckReply;

        /// <summary>
        /// Reset the handshake state on connected
        /// And reset the timer.
        /// </summary>
        public void OnConnected()
        {
            m_RemoteHello = null;
            m_RemoteCheck = m_LocalCheck = null;
            m_RemoteCheckReply = null;
            m_LocalCheckReply = null;
        }

        /// <summary>
        /// Set the timer on disconnected.
        /// </summary>
        public void OnDisconnected()
        {
            m_Retries = 0;
        }

        private Task m_TaskRecover;
        private int m_Retries;

        /// <summary>
        /// Tick. this will not be called if the connection is alive or handshaking.
        /// </summary>
        /// <param name="Incomings"></param>
        /// <returns></returns>
        public async Task TickAsync(Channel<OrpMessage> Incomings)
        {
            if (m_TaskRecover != null && m_TaskRecover.IsCompleted)
            {
                try { await m_TaskRecover; }
                catch { }

                m_TaskRecover = null;
            }

            if (m_TaskRecover is null)
                m_TaskRecover = RecoverAsync(Incomings);
        }

        /// <summary>
        /// Recover the connection.
        /// </summary>
        /// <param name="Incomings"></param>
        /// <returns></returns>
        private async Task RecoverAsync(Channel<OrpMessage> Incomings)
        {
            SetState(OrpMeshPeerState.Connecting);
            while(true)
            {
                using var Cts = new CancellationTokenSource(Mesh.Options.ConnectionTimeout);
                if (!await Connection.ConnectAsync(RemoteEndPoint, Incomings, Cts.Token))
                {
                    if (!Peers.ContainsPeer(this) || ++m_Retries >= Mesh.Options.MaxRetriesPerPeer)
                    {
                        Peers.RemovePeer(this);
                        SetState(OrpMeshPeerState.Removed);
                        break;
                    }

                    await Task.Delay(Mesh.Options.ConnectionRecoveryDelay);
                    continue;
                }

                if (!Peers.ContainsPeer(this))
                {
                    await Connection.DisconnectAsync();
                    Peers.RemovePeer(this);

                    SetState(OrpMeshPeerState.Removed);
                    break;
                }

                OnConnected();

                SetState(OrpMeshPeerState.Handshaking);
                m_LocalHello = INIT_Hello.From(Mesh);

                await Connection.EmitAsync(m_LocalHello);
                break;
            }
        }

        /// <summary>
        /// Handle Handshake Messages.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        private async Task HandshakeAsync(object Message)
        {
            var Kick = false;
            switch (Message)
            {
                case INIT_Hello Hello:
                    if (m_RemoteHello != null && !Hello.IsNetworkCompatible(Mesh.Options))
                    {
                        Kick = true;
                        break;
                    }

                    m_RemoteHello = Hello;
                    m_RemoteCheck = new INIT_Check();

                    await Connection.EmitAsync(m_RemoteCheck);
                    break;

                case INIT_Check Check: // Request from the remote.
                    if (m_LocalCheck != null)
                    {
                        Kick = true;
                        break;
                    }

                    m_LocalCheck = Check;
                    m_LocalCheckReply = INIT_CheckReply.From(Mesh.LocalMeshToken, Check);

                    await Connection.EmitAsync(m_LocalCheckReply);
                    break;

                case INIT_CheckReply CheckReply: // Reply from the remote.
                    if (m_RemoteCheckReply != null && m_RemoteHello is null)
                    {
                        Kick = true;
                        break;
                    }

                    if (!(m_RemoteCheckReply = CheckReply).Verify(m_RemoteHello.MeshToken, m_RemoteCheck))
                    {
                        Kick = true;
                        break;
                    }

                    await Connection.EmitAsync(new INIT_Done());
                    break;

                case INIT_Done:
                    if (m_RemoteHello is null || m_RemoteCheckReply is null)
                    {
                        Kick = true;
                        break;
                    }

                    RemoteMeshToken = m_RemoteHello.MeshToken;
                    RemoteAdvertisement = m_RemoteHello.Advertisement;
                    SetState(OrpMeshPeerState.Connected);
                    break;

                default:
                    Kick = true;
                    break;
            }

            if (Kick)
            {
                await Connection.DisconnectAsync();
            }
        }
    }
}
