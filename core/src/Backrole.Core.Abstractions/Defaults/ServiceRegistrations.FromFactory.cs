using System;

namespace Backrole.Core.Abstractions.Defaults
{
    public static partial class ServiceRegistrations
    {
        /// <summary>
        /// Service Instance Factory as the registration.
        /// </summary>
        internal class FromFactory : IServiceRegistration
        {
            /// <summary>
            /// Initialize a new <see cref="FromFactory"/> instance.
            /// </summary>
            /// <param name="Lifetime"></param>
            internal FromFactory(Type Type, ServiceLifetime Lifetime, Func<IServiceProvider, Type, object> Factory)
            {
                this.Type = Type;
                this.Lifetime = Lifetime;
                this.Factory = Factory;
            }

            /// <inheritdoc/>
            public Type Type { get; }

            /// <inheritdoc/>
            public bool KeepAlive { get; } = false;

            /// <inheritdoc/>
            public ServiceLifetime Lifetime { get; }

            /// <summary>
            /// Factory delegate that implements the <see cref="GetInstance(IServiceProvider, Type)"/> method.
            /// </summary>
            public Func<IServiceProvider, Type, object> Factory { get; }

            /// <inheritdoc/>
            public object GetInstance(IServiceProvider Services, Type RequestedType)
            {
                if (Factory is null)
                    throw new InvalidOperationException("Factory shouldn't be null.");

                return Factory.Invoke(Services, RequestedType);
            }
        }
    }
}
