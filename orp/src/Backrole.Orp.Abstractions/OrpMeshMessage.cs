using System;

namespace Backrole.Orp.Abstractions
{
    public struct OrpMeshMessage
    {
        /// <summary>
        /// Initialize a new <see cref="OrpMeshMessage"/> value.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="TimeStamp"></param>
        /// <param name="Message"></param>
        public OrpMeshMessage(IOrpMeshPeer Source, DateTime TimeStamp, object Message)
        {
            if (TimeStamp.Kind != DateTimeKind.Utc)
                TimeStamp = TimeStamp.ToUniversalTime();

            this.Source = Source;
            this.TimeStamp = TimeStamp;
            this.Message = Message;
        }

        /// <summary>
        /// Sender instance.
        /// </summary>
        public IOrpMeshPeer Source { get; }

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
