using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interface that creates the container.
    /// </summary>
    public interface IContainerBuilder
    {
        /// <summary>
        /// Host Services.
        /// </summary>
        IServiceProvider HostServices { get; }

        /// <summary>
        /// A central location to share objects between container services.
        /// </summary>
        IServiceProperties Properties { get; }

        /// <summary>
        /// Build an <see cref="IContainer"/> instance.
        /// </summary>
        /// <returns></returns>
        IContainer Build();
    }
}
