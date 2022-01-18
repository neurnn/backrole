using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interface that the hosted service that runs on the <see cref="IHost"/> should implement.
    /// </summary>
    public interface IHostedService
    {
        /// <summary>
        /// Start the <see cref="IHostedService"/>.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken Token = default);

        /// <summary>
        /// Stop the <see cref="IHostedService"/>.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
