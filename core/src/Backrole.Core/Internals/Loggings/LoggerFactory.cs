using Backrole.Core.Abstractions;
using Backrole.Core.Internals.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Loggings
{
    internal class LoggerFactory : ILoggerFactory, IDisposable, IAsyncDisposable
    {
        private ILoggerFactory[] m_Factories;
        private ServiceDisposables m_Disposables = new();

        /// <summary>
        /// Initialize a new <see cref="LoggerFactory"/> instance.
        /// </summary>
        /// <param name="Factories"></param>
        public LoggerFactory(ILoggerFactory[] Factories)
        {
            m_Disposables.SetPostDispose(() => m_Factories = new ILoggerFactory[0]);
            foreach (var Each in (m_Factories = Factories))
                m_Disposables.Reserve(Each);
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string Category) 
            => new Logger(m_Factories.Select(X => X.CreateLogger(Category)).ToArray());

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => m_Disposables.DisposeAsync();

    }
}
