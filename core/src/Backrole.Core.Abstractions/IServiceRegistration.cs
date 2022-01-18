using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Defines how to access to the instance including construction, life-time and its destruction.
    /// </summary>
    public interface IServiceRegistration
    {
        /// <summary>
        /// <see cref="Type"/> Information of the service instance.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// The value indicates the instance is from external and it shouldn't be disposed by the service provider or scope.
        /// But the <see cref="Lifetime"/> isn't <see cref="ServiceLifetime.Singleton"/>, this property will be ignored.
        /// </summary>
        bool KeepAlive { get; }

        /// <summary>
        /// Lifetime of the service instance.
        /// </summary>
        ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Get an instance of the service.
        /// This method have no responsibility to repeat result for the <see cref="ServiceLifetime"/>.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="RequestedType"></param>
        /// <returns></returns>
        object GetInstance(IServiceProvider Services, Type RequestedType);
    }

}
