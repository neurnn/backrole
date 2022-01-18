using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interface that all containers should implement. (including the host)
    /// </summary>
    public interface IContainer : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Service Provider instance.
        /// </summary>
        IServiceProvider Services { get; }
    }
}
