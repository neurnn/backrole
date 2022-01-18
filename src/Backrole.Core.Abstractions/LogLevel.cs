namespace Backrole.Core.Abstractions
{
    public enum LogLevel
    {
        /// <summary>
        /// Informational messages.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Trace purpose messages.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Debug purpose messages.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Warning messages.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Error messages.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Fatal failures.
        /// </summary>
        Critical = 5,
    }
}
