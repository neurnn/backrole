using System;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Receiving result.
    /// </summary>
    public struct OrpMessage
    {
        /// <summary>
        /// Initialize a new <see cref="OrpMessage"/> value.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="TimeStamp"></param>
        /// <param name="Message"></param>
        public OrpMessage(IOrpClient Source, DateTime TimeStamp, object Message)
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
        public IOrpClient Source { get; }

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
