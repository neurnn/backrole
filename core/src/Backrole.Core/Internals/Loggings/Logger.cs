using Backrole.Core.Abstractions;
using System;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Loggings
{
    internal class Logger : ILogger
    {
        private ILogger[] m_Loggers;

        /// <summary>
        /// Initialize a new <see cref="Logger"/> instance.
        /// </summary>
        /// <param name="Loggers"></param>
        public Logger(ILogger[] Loggers) => m_Loggers = Loggers;

        /// <inheritdoc/>
        public ILogger Log(LogLevel Level, string Message, Exception Error = null)
        {
            foreach (var Each in m_Loggers)
                Each.Log(Level, Message, Error);

            return this;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var Each in m_Loggers)
                Each.Dispose();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            foreach (var Each in m_Loggers)
                await Each.DisposeAsync();
        }

    }
}
