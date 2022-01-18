using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interface that creates the container builder and its result container.
    /// </summary>
    public interface IContainerBuilderFactory 
    {
        /// <summary>
        /// Create the builder instance.
        /// </summary>
        /// <param name="HostServices"></param>
        /// <returns></returns>
        IContainerBuilder Create(IServiceProvider HostServices);

        /// <summary>
        /// Build a <see cref="IContainer"/> instance.
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        IContainer Build(IContainerBuilder Builder);
    }
}
