using Backrole.Core;
using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Internals
{
    internal class HttpContainer : IHttpContainer
    {
        [ServiceInjection]
        private IServiceScope m_Scope = null;
        
        /// <inheritdoc/>
        public IServiceProvider Services => m_Scope.ServiceProvider;

        /// <inheritdoc/>
        public void Dispose() => m_Scope.Dispose();

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => m_Scope.DisposeAsync();
    }
}
