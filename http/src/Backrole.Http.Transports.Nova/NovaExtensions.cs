using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Internals;
using System;

namespace Backrole.Http.Transports.Nova
{
    public static class NovaExtensions
    {
        /// <summary>
        /// Add the Nova to accept <see cref="IHttpContext"/>s.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHttpContextTransportBuilder UseNova(this IHttpContextTransportBuilder This, Action<NovaOptions> Configure = null)
        {
            var Options = new NovaOptions();
            This.Add(Services => new NovaTransport(Services, Options));

            Configure?.Invoke(Options);
            return This;
        }
    }
}
