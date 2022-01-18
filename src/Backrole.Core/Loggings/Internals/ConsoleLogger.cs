using Backrole.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backrole.Core.Loggings.Internals
{
    internal class ConsoleLogger : ILogger
    {
        private static readonly object PADLOCK = new object();
        private static bool FIRST_LOG = true;

        private const string TAB_PARAGRAPH = "   | ";
        private const string TAB_CATEGORY = " @ ";
        private const string TAB_MESSAGE = "   : ";
        private const string TAB_EXCEPTION = " > ";

        private ConsoleLoggerOptions m_Options;
        private string m_Category;

        /// <summary>
        /// Initialize a new <see cref="ConsoleLogger"/> instance.
        /// </summary>
        /// <param name="Category"></param>
        public ConsoleLogger(string Category, ConsoleLoggerOptions Options)
        {
            m_Category = Category;
            m_Options = Options;
        }

        // ---------------------------------
        private IEnumerable<string> TabParagraph(string Log, string vTab = TAB_MESSAGE) => TabParagraph(Log.Replace("\t", m_Options.GetTabString()).Split('\n'), vTab);
        private IEnumerable<string> TabParagraph(IEnumerable<string> Lines, string vTab = TAB_MESSAGE)
        {
            var Width = Console.WindowWidth;
            if (vTab is null)
                vTab = TAB_MESSAGE;

            foreach (var Each in Lines)
            {
                var Tab = vTab;
                var Temp = Each;

                while (Temp.Length > (Width - Tab.Length))
                {
                    var Line = Temp.Substring(0, Width - Tab.Length);
                    Temp = Temp.Substring(Line.Length);

                    yield return $"{Tab}{Line}";

                    if (Tab == vTab)
                        Tab = TAB_PARAGRAPH;
                }

                if (Temp.Length > 0)
                {
                    yield return $"{Tab}{Temp}";
                    vTab = TAB_PARAGRAPH;
                }
            }
        }

        /// <inheritdoc/>
        public ILogger Log(LogLevel Level, string Message, Exception Error = null)
        {
            if (!m_Options.LogLevels.Contains(Level))
                return this;

            lock (PADLOCK)
            {
                var Now = DateTime.Now;
                var Dest = Level != LogLevel.Information
                    ? Console.Error : Console.Out;

                if (m_Options.PrintBoundaries)
                {
                    if (!FIRST_LOG)
                        Dest.WriteLine(m_Options.BoundaryString);

                    FIRST_LOG = false;
                }

                PrintLogHeaders(Level, Now, Dest);
                PrintLogMessage(Message, Error, Dest);
            }

            return this;
        }

        /// <summary>
        /// Print the log message as formatted.
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <param name="Dest"></param>
        private void PrintLogMessage(string Message, Exception Error, TextWriter Dest)
        {
            if (Console.IsOutputRedirected || !m_Options.PrettyPrint)
            {
                Dest.WriteLine($"{TAB_MESSAGE}{Message}");
                if (Error != null)
                    Dest.WriteLine($"{TAB_EXCEPTION}{Error}");
            }

            else
            {
                Render(Dest, TabParagraph(Message, TAB_MESSAGE));
                if (Error != null)
                {
                    var ErrorTrace = Error.ToString()
                        .Split('\n').Select(X => X.Trim());

                    if (!m_Options.HighlightException)
                        Render(Dest, TabParagraph(ErrorTrace, TAB_EXCEPTION));

                    else
                    {
                        using (new Colorize(m_Options.HighlightColorset, m_Options))
                            Render(Dest, TabParagraph(ErrorTrace, TAB_EXCEPTION));
                    }
                }
            }
        }

        /// <summary>
        /// Prints the log headers that allows to recognize when the log message generated, its category and its level.
        /// </summary>
        /// <param name="Level"></param>
        /// <param name="Now"></param>
        /// <param name="Dest"></param>
        private void PrintLogHeaders(LogLevel Level, DateTime Now, TextWriter Dest)
        {
            if (m_Options.PrettyPrint)
                m_Options.ResetConsole();

            m_Options.Colors.TryGetValue(Level, out var Color);
            if (Console.IsOutputRedirected || !m_Options.PrettyPrint)
                Dest.Write(Color.Text);

            else
            {
                using (new Colorize(Color, m_Options))
                    Dest.Write(Color.Text);
            }

            Dest.WriteLine($" {Now.ToString(m_Options.DateFormatString)} ");
            if (!string.IsNullOrWhiteSpace(m_Category))
                Dest.WriteLine($"{TAB_CATEGORY}{m_Category}");
        }

        /// <summary>
        /// Render all lines with padding spaces.
        /// </summary>
        /// <param name="Dest"></param>
        /// <param name="Lines"></param>
        private static void Render(TextWriter Dest, IEnumerable<string> Lines)
        {
            foreach (var Line in Lines)
                Render(Dest, Line);
        }

        /// <summary>
        /// Render a line with padding spaces.
        /// </summary>
        /// <param name="Dest"></param>
        /// <param name="Line"></param>
        private static void Render(TextWriter Dest, string Line)
        {
            Dest.Write(Line);

            var PadSize = Math.Max(Console.WindowWidth - Line.Length, 0);
            if (PadSize > 0)
                Dest.WriteLine(new string(' ', PadSize));
        }

        private struct Colorize : IDisposable
        {
            private ConsoleLoggerOptions m_Options;

            public Colorize(ConsoleLoggerColor Color, ConsoleLoggerOptions Options)
            {
                m_Options = Options;

                Console.BackgroundColor = Color.Background;
                Console.ForegroundColor = Color.Foreground;
            }

            public void Dispose()
            {
                Console.BackgroundColor = m_Options.Background;
                Console.ForegroundColor = m_Options.Foreground;
            }
        }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
