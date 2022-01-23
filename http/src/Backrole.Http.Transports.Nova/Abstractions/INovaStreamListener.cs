using Backrole.Http.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Abstractions
{
    /// <summary>
    /// Listens the <see cref="INovaStream"/> instance.
    /// </summary>
    public interface INovaStreamListener
    {
        /// <summary>
        /// Listen Mode of the listener.
        /// </summary>
        NovaListenMode ListenMode { get; }

        /// <summary>
        /// Start to listen incoming streams.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop listening incoming streams.
        /// </summary>
        void Stop();

        /// <summary>
        /// Accept an <see cref="INovaStream"/> instance.
        /// </summary>
        /// <returns></returns>
        Task<INovaStream> AcceptAsync(IHttpServiceProvider HttpServices);
    }
}
