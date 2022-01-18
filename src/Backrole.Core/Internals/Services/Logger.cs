using Backrole.Core.Abstractions;
using System;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Services
{
    internal class Logger<T> : ILogger<T>
    {
        private ILogger m_Logger;

        /// <summary>
        /// Initialize a new <see cref="Logger{T}"/> instance.
        /// </summary>
        /// <param name="LoggerFactory"></param>
        public Logger(ILoggerFactory LoggerFactory)
            => m_Logger = LoggerFactory.CreateLogger(typeof(T).FullName);

        /// <inheritdoc/>
        public ILogger<T> Log(LogLevel Level, string Message, Exception Error = null)
        {
            m_Logger.Log(Level, Message, Error);
            return this;
        }

        /// <inheritdoc/>
        ILogger ILogger.Log(LogLevel Level, string Message, Exception Error) => Log(Level, Message, Error);

        /// <inheritdoc/>
        public void Dispose() => m_Logger.Dispose();

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => m_Logger.DisposeAsync();

    }
}
