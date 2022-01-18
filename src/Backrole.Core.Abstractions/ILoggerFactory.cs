namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Factory interface that creates ILogger instances to cover logger service requests.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Create a logger with <paramref name="Category"/> name.
        /// </summary>
        /// <param name="Category"></param>
        /// <returns></returns>
        ILogger CreateLogger(string Category);
    }
}
