using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.HttpSys.Internals
{
    internal class HttpSysWebSocketFeature : IHttpWebSocketFeature
    {
        private bool? m_CanUpgrade;

        /// <summary>
        /// Initialize a new <see cref="HttpSysWebSocketFeature"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        public HttpSysWebSocketFeature(IServiceProvider Services) 
            => Request = Services.GetRequiredService<IHttpRequest>();

        /// <inheritdoc/>
        public IHttpRequest Request { get; }

        /// <inheritdoc/>
        public IHttpServiceProvider Services => Request.Context.Services;

        /// <inheritdoc/>
        public IHttpConnectionInfo Connection => Request.Context.Connection;

        /// <inheritdoc/>
        public bool CanUpgrade
        {
            get
            {
                if (!m_CanUpgrade.HasValue)
                {
                    if (!Request.Context.Properties
                        .TryGetValue<HttpListenerContext>(typeof(HttpListenerContext), out var Context) ||
                        Context is null)
                    {
                        return (m_CanUpgrade = false).Value;
                    }

                    m_CanUpgrade = Context.Request.IsWebSocketRequest;
                }

                return m_CanUpgrade.Value;
            }
        }

        /// <inheritdoc/>
        public async Task<WebSocket> UpgradeAsync(string Subprotocol = null)
        {
            var Properties = Request.Context.Properties;

            if (CanUpgrade && /* The reason why get the Http.Sys context from properties is avoiding the opaque wrapping. */
                Properties.TryGetValue<HttpListenerContext>(typeof(HttpListenerContext), out var Context) &&
                Context != null)
            {
                var WSContext = await Context.AcceptWebSocketAsync(Subprotocol, TimeSpan.FromSeconds(1));
                if (WSContext != null) return WSContext.WebSocket;
            }

            return null;
        }
    }
}
