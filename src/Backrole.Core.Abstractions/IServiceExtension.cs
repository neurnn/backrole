using System;
using System.Reflection;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Defines the interface that the extension should have.
    /// Note that all extensions shouldn't be disposable.
    /// </summary>
    public interface IServiceExtension
    {
        /// <summary>
        /// Get Service Instance by <paramref name="RequestedType"/> and <see cref="IServiceScope"/>.
        /// The results of this operation are not cached. It also shouldn't be cached.
        /// Note that this method will be called to handle the service requests.
        /// This can override any type of services and if this returns null, the procedure will continue to default operation.
        /// </summary>
        /// <param name="RequestedType"></param>
        /// <param name="ScopedServices"></param>
        /// <returns></returns>
        object GetService(IServiceProvider ScopedServices, Type RequestedType);

        /// <summary>
        /// Get Service Instance for injecting some properties or fields with <paramref name="TargetType"/>, <paramref name="Attributes"/> and <see cref="IServiceScope"/>.
        /// The results of this operation are not cached. It also shouldn't be cached.
        /// Note that this method will be called to resolve services for properties or fields.
        /// This can override any type of services and if this returns null, the procedure will continue to default operation.
        /// </summary>
        /// <param name="ScopedServices"></param>
        /// <param name="TargetType"></param>
        /// <param name="Attributes"></param>
        /// <returns></returns>
        object GetService(IServiceProvider ScopedServices, Type TargetType, ICustomAttributeProvider Attributes);
    }

}
