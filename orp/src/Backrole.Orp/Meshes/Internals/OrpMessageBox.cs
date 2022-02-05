using Backrole.Orp.Abstractions;
using Backrole.Orp.Meshes.Internals.A_Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Backrole.Orp.Meshes.Internals
{
    class OrpMessageBox
    {
        private Channel<OrpMeshMessage> m_Channel;

        public void Start()
        {
            m_Channel = Channel.CreateUnbounded<OrpMeshMessage>();
        }

        public void Stop()
        {
            m_Channel.Writer.TryComplete();
        }

        public async Task OnMessageAsync(IOrpMeshPeer Peer, object Message)
        {
            if (m_Channel is null)
                return;

            try { await m_Channel.Writer.WriteAsync(new OrpMeshMessage(Peer, DateTime.UtcNow, Message)); }
            catch
            {
            }
        }

        public async Task<OrpMeshMessage> WaitAsync(CancellationToken Token = default)
        {
            if (m_Channel != null)
            {
                try { return await m_Channel.Reader.ReadAsync(Token); }
                catch
                {

                }
            }

            throw new InvalidOperationException("the server isn't started yet.");
        }
    }
}
