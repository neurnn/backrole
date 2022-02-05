using System;
using System.Threading.Tasks;

namespace Backrole.Orp.Abstractions
{
    public interface IOrpMeshProtocolModule
    {
        /// <summary>
        /// Called when peer status changed.
        /// </summary>
        /// <param name="Peer"></param>
        void OnStatusChanged(IOrpMeshPeer Peer);

        /// <summary>
        /// Called when a new message arrived.
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Next"></param>
        /// <returns></returns>
        Task OnMessageAsync(OrpMessage Message, Func<Task> Next);
    }
}
