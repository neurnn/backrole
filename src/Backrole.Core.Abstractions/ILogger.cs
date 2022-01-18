using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An abstract interface that writes logs for error situations that occur while the container is running.
    /// </summary>
    public interface ILogger : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Write a log message.
        /// </summary>
        /// <param name="Level"></param>
        /// <param name="Message">Message to log.</param>
        /// <param name="Error">Specifies the related <see cref="Exception"/> with the message.</param>
        /// <returns></returns>
        ILogger Log(LogLevel Level, string Message, Exception Error = null);
    }

    /// <summary>
    /// An abstract interface that writes logs for error situations that occur while the container is running.
    /// (Generic version of <see cref="ILogger"/> interface)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILogger<T> : ILogger
    {
        /// <summary>
        /// Write a log message.
        /// </summary>
        /// <param name="Level"></param>
        /// <param name="Message">Message to log.</param>
        /// <param name="Error">Specifies the related <see cref="Exception"/> with the message.</param>
        /// <returns></returns>
        new ILogger<T> Log(LogLevel Level, string Message, Exception Error = null);
    }
}
