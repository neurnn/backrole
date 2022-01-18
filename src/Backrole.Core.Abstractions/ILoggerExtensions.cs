using System;

namespace Backrole.Core.Abstractions
{
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Write a trace message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger Trace(this ILogger Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Trace, Message, Error);

        /// <summary>
        /// Write a debug message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger Debug(this ILogger Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Debug, Message, Error);

        /// <summary>
        /// Write a information message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger Info(this ILogger Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Information, Message, Error);

        /// <summary>
        /// Write an warning message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger Warn(this ILogger Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Warning, Message, Error);

        /// <summary>
        /// Write an error message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger Error(this ILogger Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Error, Message, Error);

        /// <summary>
        /// Write an fatal message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger Fatal(this ILogger Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Critical, Message, Error);

        /// <summary>
        /// Write a trace message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger<T> Trace<T>(this ILogger<T> Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Trace, Message, Error);

        /// <summary>
        /// Write a debug message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger<T> Debug<T>(this ILogger<T> Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Debug, Message, Error);

        /// <summary>
        /// Write a information message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger<T> Info<T>(this ILogger<T> Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Information, Message, Error);

        /// <summary>
        /// Write an warning message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger<T> Warn<T>(this ILogger<T> Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Warning, Message, Error);

        /// <summary>
        /// Write an error message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger<T> Error<T>(this ILogger<T> Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Error, Message, Error);

        /// <summary>
        /// Write an fatal message.
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static ILogger<T> Fatal<T>(this ILogger<T> Logger, string Message, Exception Error = null)
            => Logger.Log(LogLevel.Critical, Message, Error);
    }
}
