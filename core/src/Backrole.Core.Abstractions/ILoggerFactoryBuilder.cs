using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Builds the <see cref="ILoggerFactory"/> instance for the entire container.
    /// </summary>
    public interface ILoggerFactoryBuilder
    {
        /// <summary>
        /// Service collection that the logger will be configured.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Remove all added factory delegates from the builder.
        /// </summary>
        /// <returns></returns>
        ILoggerFactoryBuilder Clear();

        /// <summary>
        /// Adds a factory delegate that creates the <see cref="ILoggerFactory"/> instance.
        /// </summary>
        /// <returns></returns>
        ILoggerFactoryBuilder Add(Func<IServiceProvider, ILoggerFactory> Delegate);

        /// <summary>
        /// Build an <see cref="ILoggerFactory"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <returns></returns>
        ILoggerFactory Build(IServiceProvider Services);
    }
}
