using System;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Options interface.
    /// </summary>
    public interface IOrpReadOnlyOptions
    {
        /// <summary>
        /// Epoch of the protocol.
        /// </summary>
        DateTime Epoch { get; }

        /// <summary>
        /// Decides whether the ORP protocol uses little endian or not.
        /// </summary>
        bool UseLittleEndian { get; }

        /// <summary>
        /// Size of the incoming message queue.
        /// </summary>
        int IncomingQueueSize { get; }

        /// <summary>
        /// Try to find the type from its name.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        bool TryGetType(string Name, out Type Type);

        /// <summary>
        /// Try to find the name of the type.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        bool TryGetName(Type Type, out string Name);
    }
}
