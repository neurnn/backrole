using System;

namespace Backrole.Core.Abstractions.Defaults
{
    public static partial class ServiceRegistrations
    {
        /// <summary>
        /// Singleton Instance itself as the registration.
        /// </summary>
        internal class FromInstance : IServiceRegistration
        {
            /// <summary>
            /// Initialize a new <see cref="FromInstance"/> instance.
            /// </summary>
            /// <param name="Instance"></param>
            internal FromInstance(Type Type, object Instance, bool KeepAlive = false)
            {
                this.Type = Type;
                this.Instance = Instance;
                this.KeepAlive = KeepAlive;
            }

            /// <inheritdoc/>
            public Type Type { get; }

            /// <inheritdoc/>
            public bool KeepAlive { get; }

            /// <inheritdoc/>
            public ServiceLifetime Lifetime { get; } = ServiceLifetime.Singleton;

            /// <inheritdoc/>
            public object Instance { get; }

            /// <inheritdoc/>
            public object GetInstance(IServiceProvider Services, Type RequestedType) => Instance;
        }
    }
}
