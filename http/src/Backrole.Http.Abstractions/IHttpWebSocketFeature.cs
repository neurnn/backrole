using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Http WebSocket feature interface.
    /// </summary>
    public interface IHttpWebSocketFeature
    {
        /// <summary>
        /// Request instance.
        /// </summary>
        IHttpRequest Request { get; }

        /// <summary>
        /// Http Services.
        /// </summary>
        IHttpServiceProvider Services { get; }

        /// <summary>
        /// Connection information.
        /// </summary>
        IHttpConnectionInfo Connection { get; }

        /// <summary>
        /// Test whether the request can be upgraded to WebSocket or not.
        /// </summary>
        bool CanUpgrade { get; }

        /// <summary>
        /// Accept the WebSocket request.
        /// </summary>
        /// <param name="Subprotocol"></param>
        /// <returns></returns>
        Task<WebSocket> UpgradeAsync(string Subprotocol = null);
    }
}
