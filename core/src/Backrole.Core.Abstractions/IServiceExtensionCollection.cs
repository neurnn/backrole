using System;
using System.Collections.Generic;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Extension Collection that registers all extensions to extend the service provider itself.
    /// </summary>
    public interface IServiceExtensionCollection : ICollection<IServiceExtension>
    {
        /// <summary>
        /// Find an extension by its type from front of the collection.
        /// </summary>
        /// <param name="ExtensionType"></param>
        /// <returns></returns>
        IServiceExtension Find(Type ExtensionType);

        /// <summary>
        /// Find an extension by its type from back of the collection.
        /// </summary>
        /// <param name="ExtensionType"></param>
        /// <returns></returns>
        IServiceExtension FindLast(Type ExtensionType);
    }

}
