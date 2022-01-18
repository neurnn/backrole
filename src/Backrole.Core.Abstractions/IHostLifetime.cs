using System.Threading;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interface that notify the host's lifetime events.
    /// </summary>
    public interface IHostLifetime
    {
        /// <summary>
        /// Triggered when the host is started.
        /// </summary>
        CancellationToken Started { get; }

        /// <summary>
        /// Triggered when the host is stopping,
        /// </summary>
        CancellationToken Stopping { get; }

        /// <summary>
        /// Triggered when the host is stopped.
        /// </summary>
        CancellationToken Stopped { get; }

        /// <summary>
        /// Request to stop the host.
        /// </summary>
        void Stop();
    }
}
