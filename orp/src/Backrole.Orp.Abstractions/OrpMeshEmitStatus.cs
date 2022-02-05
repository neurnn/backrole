using System;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Emitting status of the message.
    /// </summary>
    public struct OrpMeshEmitStatus
    {
        /// <summary>
        /// Initialize a new <see cref="OrpEmitStatus"/> value.
        /// </summary>
        /// <param name="Destination"></param>
        /// <param name="TimeStamp"></param>
        /// <param name="Message"></param>
        public OrpMeshEmitStatus(IOrpMeshPeer Destination, DateTime TimeStamp, object Message)
        {
            if (TimeStamp.Kind != DateTimeKind.Utc)
                TimeStamp = TimeStamp.ToUniversalTime();

            this.Destination = Destination;
            this.TimeStamp = TimeStamp;
            this.Message = Message;
        }

        /// <summary>
        /// Destinations who will receive the message.
        /// </summary>
        public IOrpMeshPeer Destination { get; }

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
