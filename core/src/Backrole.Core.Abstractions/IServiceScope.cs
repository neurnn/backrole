using System;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Service scope that is branched from the parent service provider.
    /// </summary>
    public interface IServiceScope : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Service Provider instance.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}
