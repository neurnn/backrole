using Backrole.Core.Abstractions;
using Backrole.Core.Internals.Services;
using System;
using System.Threading.Tasks;

namespace Backrole.Core
{
    /// <summary>
    /// Backrole's implementation of the <see cref="IServiceProvider"/>.
    /// </summary>
    public sealed class ServiceProvider : IServiceProvider, IAsyncDisposable, IDisposable
    {
        private IServiceScope m_Scope;
        private ValueTask? m_Dispose;

        /// <summary>
        /// Initialize a new <see cref="ServiceProvider"/> using the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="Services"></param>
        public ServiceProvider(IServiceCollection Services) => m_Scope = new ServiceScope(Services);

        /// <inheritdoc/>
        public object GetService(Type ServiceType)
            => m_Scope.ServiceProvider.GetService(ServiceType);

        /// <summary>
        /// Dispose the <see cref="ServiceProvider"/> instance.
        /// </summary>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Dispose the <see cref="ServiceProvider"/> instance.
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            lock(this)
            {
                if (m_Dispose.HasValue)
                    return m_Dispose.Value;

                return (m_Dispose = m_Scope.DisposeAsync()).Value;
            }
        }
    }
}
