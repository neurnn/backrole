using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Creates a service scope instance that is branched from the current service provider.
    /// </summary>
    public interface IServiceScopeFactory
    {
        /// <summary>
        /// Create a new <see cref="IServiceScope"/> instance.
        /// </summary>
        /// <param name="Overrides">Overrides the scope specific services.</param>
        /// <returns></returns>
        IServiceScope CreateScope(Action<IServiceCollection> Overrides = null);

        /// <summary>
        /// Create a new <see cref="IServiceScope"/> instance.
        /// </summary>
        /// <param name="Overrides">Overrides the scope specific services.</param>
        /// <returns></returns>
        IServiceScope CreateScope(IServiceProperties Properties, Action<IServiceCollection> Overrides);
    }
}
