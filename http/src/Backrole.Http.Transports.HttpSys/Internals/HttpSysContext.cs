using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using Backrole.Http.Abstractions;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.HttpSys.Internals
{
    internal class HttpSysContext : IHttpContext
    {
        /// <summary>
        /// Initialize a new <see cref="HttpSysContext"/> instance.
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="Services"></param>
        public HttpSysContext(HttpListenerContext Context, IHttpServiceProvider Services, CancellationToken Aborted)
        {
            this.Context = Context;
            this.Services = Services;
            this.Aborted = Aborted;
            
            Request = new HttpSysRequest(this);
            Response = new HttpSysResponse(this);
            Connection = new HttpSysConnectionInfo(Context);

            // The reason why put the Http.Sys context to properties is avoiding the opaque wrapping.
            Properties[typeof(HttpListenerContext)] = Context;
        }

        /// <summary>
        /// <see cref="HttpListenerContext"/> instance.
        /// </summary>
        public HttpListenerContext Context { get; }

        /// <inheritdoc/>
        public IServiceProperties Properties { get; } = new ServiceProperties();

        /// <inheritdoc/>
        public IHttpServiceProvider Services { get; }

        /// <inheritdoc/>
        public IHttpConnectionInfo Connection { get; }

        /// <inheritdoc/>
        public IHttpRequest Request { get; }

        /// <inheritdoc/>
        public IHttpResponse Response { get; }

        /// <inheritdoc/>
        public CancellationToken Aborted { get; }

        /// <summary>
        /// Called when the response should be sent.
        /// </summary>
        /// <returns></returns>
        public Task OnCompleteAsync()
        {
            if (Response is HttpSysResponse Http)
                return Http.SendAsync();

            return Task.CompletedTask;
        }
    }
}
