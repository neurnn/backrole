using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova
{
    /// <summary>
    /// Defines the mode of Nova WebServer implementation.
    /// </summary>
    public enum NovaListenMode
    {
        /// <summary>
        /// Http 1.x.
        /// </summary>
        Http1,

        /// <summary>
        /// Http 1.x with h2c
        /// </summary>
        Http1AndHttp2,

        /// <summary>
        /// Http 2 only..
        /// </summary>
        Http2
    }
}
