using Backrole.Core.Abstractions;
using System;

namespace Backrole.Core.Loggings
{
    public struct ConsoleLoggerColor
    {
        /// <summary>
        /// Initialize a new <see cref="ConsoleLoggerColor"/>.
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Foreground"></param>
        /// <param name="Background"></param>
        public ConsoleLoggerColor(string Text, ConsoleColor Background, ConsoleColor Foreground)
        {
            this.Text = Text;
            this.Foreground = Foreground;
            this.Background = Background;
        }

        /// <summary>
        /// Text expression of the <see cref="LogLevel"/>.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Background color of the <see cref="LogLevel"/> text.
        /// </summary>
        public ConsoleColor Background { get; }

        /// <summary>
        /// Foreground color of the <see cref="LogLevel"/> text.
        /// </summary>
        public ConsoleColor Foreground { get; }
    }
}
