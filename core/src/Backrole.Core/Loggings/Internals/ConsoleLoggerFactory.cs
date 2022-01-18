using Backrole.Core.Abstractions;
using System.Diagnostics;

namespace Backrole.Core.Loggings.Internals
{
    internal class ConsoleLoggerFactory : ILoggerFactory
    {
        private ConsoleLoggerOptions m_Options;

        /// <summary>
        /// Initialize a new <see cref="ConsoleLoggerFactory"/>.
        /// </summary>
        /// <param name="Options"></param>
        public ConsoleLoggerFactory(ConsoleLoggerOptions Options)
        {
            (m_Options = Options).ResetConsole();

            if (Options.DebugLogsOnlyWithDebugger && !Debugger.IsAttached)
                Options.LogLevels.Remove(LogLevel.Debug);
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string Category) => new ConsoleLogger(Category, m_Options);
    }
}
