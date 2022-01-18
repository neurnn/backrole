using Backrole.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace Backrole.Core.Loggings
{
    /// <summary>
    /// Options that controls the output of the console logger.
    /// </summary>
    public sealed class ConsoleLoggerOptions
    {
        /// <summary>
        /// Count of [SP] characters that replaces [TAB] character.
        /// </summary>
        public int TabSpace { get; set; } = 4;

        /// <summary>
        /// Format of <see cref="DateTime"/>.
        /// </summary>
        public string DateFormatString { get; set; } = "O";

        /// <summary>
        /// Decides the log messages rendered colorfully or not.
        /// </summary>
        public bool PrettyPrint { get; set; } = true;

        /// <summary>
        /// Decides the exception part of the log message should be highlighted or not.
        /// (if the <see cref="PrettyPrint"/> set false, this will be ignored)
        /// </summary>
        public bool HighlightException { get; set; } = true;

        /// <summary>
        /// Decides the `Debug` leveled messages should be suppressed if no debugger represents.
        /// </summary>
        public bool DebugLogsOnlyWithDebugger { get; set; } = false;

        /// <summary>
        /// Print the boundary string between log messages or not.
        /// This can be used to cut messages to postprocess using terminal pipelinings.
        /// </summary>
        public bool PrintBoundaries { get; set; } = false;

        /// <summary>
        /// Boundary String that will be printed between log messages as separator if <see cref="PrintBoundaries"/> is true.
        /// </summary>
        public string BoundaryString { get; set; } = "----";

        /// <summary>
        /// Log Levels that displayed on the console.
        /// If empty, the logger will not display any logs.
        /// </summary>
        public IList<LogLevel> LogLevels { get; set; } = new List<LogLevel>()
        {
            { LogLevel.Debug },
            { LogLevel.Information },
            { LogLevel.Warning },
            { LogLevel.Error },
            { LogLevel.Critical }
        };

        /// <summary>
        /// Background color of the console window.
        /// </summary>
        public ConsoleColor Background { get; set; } = ConsoleColor.Black;

        /// <summary>
        /// Foreground color of the console window.
        /// </summary>
        public ConsoleColor Foreground { get; set; } = ConsoleColor.White;

        /// <summary>
        /// Highlight Style.
        /// </summary>
        public ConsoleLoggerColor HighlightColorset { get; set; } 
            = new ConsoleLoggerColor("", ConsoleColor.DarkRed, ConsoleColor.White);

        /// <summary>
        /// Colors that used to display log messages to console.
        /// </summary>
        public IDictionary<LogLevel, ConsoleLoggerColor> Colors { get; } = new Dictionary<LogLevel, ConsoleLoggerColor>()
        {
            { LogLevel.Trace,       new ConsoleLoggerColor("trce", ConsoleColor.Black,   ConsoleColor.Magenta)  },
            { LogLevel.Debug,       new ConsoleLoggerColor("dbug", ConsoleColor.Black,   ConsoleColor.Cyan)  },
            { LogLevel.Information, new ConsoleLoggerColor("info", ConsoleColor.Black,   ConsoleColor.Green)    },
            { LogLevel.Warning,     new ConsoleLoggerColor("warn", ConsoleColor.Black,   ConsoleColor.Yellow)   },
            { LogLevel.Error,       new ConsoleLoggerColor("erro", ConsoleColor.Black,   ConsoleColor.Red)      },
            { LogLevel.Critical,    new ConsoleLoggerColor("crit", ConsoleColor.DarkRed, ConsoleColor.White)    }
        };

        // ---------------------------------
        private string m_TabCache = null;
        private int m_ColorCache = 0;

        /// <summary>
        /// Get the replacement string of the [TAB] character.
        /// </summary>
        /// <returns></returns>
        internal string GetTabString()
        {
            lock (this)
            {
                if (m_TabCache is null || m_TabCache.Length != TabSpace)
                {
                    m_TabCache = "";

                    for (var i = 0; i < TabSpace; ++i)
                        m_TabCache += " ";
                }

                return m_TabCache;
            }
        }

        /// <summary>
        /// Reset the console as configured color.
        /// </summary>
        internal void ResetConsole()
        {
            if (!Console.IsOutputRedirected)
            {
                var Color = ((int)Background << 16) | ((int)Foreground << 16);
                if (m_ColorCache != Color)
                {
                    m_ColorCache = Color;
                    Console.BackgroundColor = Background;
                    Console.ForegroundColor = Foreground;

                    if (Background != ConsoleColor.Black)
                        Console.Clear();
                }
            }
        }
    }
}
