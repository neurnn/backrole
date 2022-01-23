using Backrole.Http.Abstractions;
using System.Net;

namespace Backrole.Http.Transports.HttpSys.Internals
{
    internal class HttpSysConnectionInfo : IHttpConnectionInfo
    {
        public HttpSysConnectionInfo(HttpListenerContext Context)
        {
            LocalAddress = Context.Request.LocalEndPoint.ToString();
            RemoteAddress = Context.Request.RemoteEndPoint.ToString();
        }

        /// <inheritdoc/>
        public string LocalAddress { get; }

        /// <inheritdoc/>
        public string RemoteAddress { get; }
    }
}
