using Backrole.Http.Abstractions;
using Backrole.Http.Transports.HttpSys.Internals;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.HttpSys
{
    public static class HttpSysExtensions
    {
        /// <summary>
        /// Add the Http Transport that wraps <see cref="HttpListener"/> (HTTP.sys) APIs.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHttpContextTransportBuilder UseHttpSys(this IHttpContextTransportBuilder This, Action<HttpSysOptions> Configure = null)
        {
            var Options = new HttpSysOptions();
            This.Add(Services => new HttpSysTransport(Services, Options));

            Configure?.Invoke(Options);
            return This;
        }

        /// <summary>
        /// Add the Http Transport that wraps <see cref="HttpListener"/> (HTTP.sys) APIs.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHttpContextTransportBuilder UseHttpSys(this IHttpContextTransportBuilder This, params string[] Prefixes)
        {
            var Options = new HttpSysOptions();
            This.Add(Services => new HttpSysTransport(Services, Options));

            Options.Clear();
            foreach (var Each in Prefixes)
                Options.Add(Each);

            return This;
        }
    }
}
