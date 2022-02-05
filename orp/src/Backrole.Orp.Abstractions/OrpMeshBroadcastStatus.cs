using System;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Emitting status of the message.
    /// </summary>
    public struct OrpMeshBroadcastStatus
    {
        /// <summary>
        /// Initialize a new <see cref="OrpBroadcastStatus"/> value.
        /// </summary>
        /// <param name="Destinations"></param>
        /// <param name="TimeStamp"></param>
        /// <param name="Message"></param>
        public OrpMeshBroadcastStatus(IOrpMeshPeer[] Destinations, DateTime TimeStamp, object Message)
        {
            if (TimeStamp.Kind != DateTimeKind.Utc)
                TimeStamp = TimeStamp.ToUniversalTime();

            this.Destinations = Destinations;
            this.TimeStamp = TimeStamp;
            this.Message = Message;
        }

        /// <summary>
        /// Destinations who will receive the message.
        /// </summary>
        public IOrpMeshPeer[] Destinations { get; }

        /// <summary>
        /// TimeStamp of the message. (UTC)
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        /// Message instance.
        /// </summary>
        public object Message { get; }
    }
}
