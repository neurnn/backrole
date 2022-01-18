using System;

namespace Backrole.Core.Internals.Services
{
    /// <summary>
    /// Opacity of the <see cref="ServiceScope"/> to prevent the user calling <see cref="IDisposable"/> methods.
    /// </summary>
    internal class ServiceOpaqueProvider : IServiceProvider
    {
        private ServiceScope m_Scope;

        /// <summary>
        /// Initialize a new <see cref="ServiceOpaqueProvider"/>.
        /// </summary>
        /// <param name="Scope"></param>
        public ServiceOpaqueProvider(ServiceScope Scope) => m_Scope = Scope;

        /// <inheritdoc/>
        public object GetService(Type ServiceType) => m_Scope.GetService(ServiceType);
    }
}
