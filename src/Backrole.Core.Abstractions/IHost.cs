using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interface that the host should implement.
    /// </summary>
    public interface IHost : IContainer
    {
        /// <summary>
        /// Containers that created for the host.
        /// </summary>
        IEnumerable<IContainer> Containers { get; }

        /// <summary>
        /// Start the <see cref="IHost"/> and its backgrounds.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken Token = default);

        /// <summary>
        /// Stop the <see cref="IHost"/> and its backgrounds.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }

    
}
