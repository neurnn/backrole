using Backrole.Core.Abstractions;

namespace Backrole.Core.Internals.Fallbacks
{
    internal class NullLoggerFactory : ILoggerFactory
    {
        /// <inheritdoc/>
        public ILogger CreateLogger(string Category) => new NullLogger();
    }
}
