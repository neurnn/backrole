using System;
using System.Collections.Generic;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Service Collection that registers all services to be served by the service provider.
    /// </summary>
    public interface IServiceCollection : ICollection<IServiceRegistration>
    {
        /// <summary>
        /// A central location to share data between service instance registrations. 
        /// </summary>
        IServiceProperties Properties { get; }

        /// <summary>
        /// Registry of the <see cref="IServiceExtension"/> that applied for the entire container.
        /// </summary>
        IServiceExtensionCollection Extensions { get; }

        /// <summary>
        /// Gets the configurator delegate that added by the <see cref="Configure(Action{IServiceProvider})"/> method.
        /// </summary>
        /// <returns></returns>
        Action<IServiceProvider> ConfigureDelegate { get; }

        /// <summary>
        /// Find a service by its service type from front of the collection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        IServiceRegistration Find(Type ServiceType);

        /// <summary>
        /// Find a service by its service type from back of the collection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        IServiceRegistration FindLast(Type ServiceType);

        /// <summary>
        /// Adds a <paramref name="Delegate"/> to <see cref="IServiceCollection"/> that invoked to configure the <see cref="IOptions{ValueType}"/>.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IServiceCollection Configure(Action<IServiceProvider> Delegate);

    }
}
