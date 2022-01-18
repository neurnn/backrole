using Backrole.Core.Abstractions;
using System;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Fallbacks
{
    internal class NullLogger : ILogger
    {
        /// <inheritdoc/>
        public ILogger Log(LogLevel Level, string Message, Exception Error = null) => this;

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
