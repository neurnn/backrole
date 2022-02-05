using System;
using System.Reflection;

namespace Backrole.Orp.Abstractions
{
    public interface IOrpOptions : IOrpReadOnlyOptions
    {
        /// <summary>
        /// Epoch of the protocol.
        /// </summary>
        new DateTime Epoch { get; set; }

        /// <summary>
        /// Decides whether the ORP protocol uses little endian or not.
        /// </summary>
        new bool UseLittleEndian { get; set; }

        /// <summary>
        /// Size of the incoming message queue.
        /// </summary>
        new int IncomingQueueSize { get; set; }

        /// <summary>
        /// Allow the user to create options in single-line.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IOrpOptions With(Action<IOrpOptions> Delegate);

        /// <summary>
        /// Add a type to map (Name:Type) tuples.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        IOrpOptions Map(Type Type, bool Override = false);

        /// <summary>
        /// Add all types that is defined on the assembly.
        /// </summary>
        /// <param name="Assembly"></param>
        /// <param name="Filter"></param>
        /// <returns></returns>
        IOrpOptions Map(Assembly Assembly, Predicate<Type> Filter = null, bool Override = false);

        /// <summary>
        /// Remove a type from map (Name:Type) tuples.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        IOrpOptions Unmap(Type Type);
    }
}
