using Backrole.Http.Abstractions;
using Backrole.Http.Abstractions.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;

namespace Backrole.Http.Transports.HttpSys.Internals
{
    internal class HttpSysRequest : IHttpRequest
    {
        private static readonly byte[] EMPTY_BODY = new byte[0];

        /// <summary>
        /// Initialize a new <see cref="HttpSysRequest"/> instance.
        /// </summary>
        /// <param name="Context"></param>
        public HttpSysRequest(HttpSysContext Context)
        {
            var Request = Context.Context.Request;
            this.Context = Context;

            var Temp = (Request.RawUrl ?? "/").Split('?', 2, StringSplitOptions.None);
            QueryString = Temp.Length <= 1 ? string.Empty : Temp.Last();
            PathString = Temp.FirstOrDefault() ?? "/";

            Method = (Request.HttpMethod ?? "").ToUpper().Trim();
            Protocol = $"HTTP/{Request.ProtocolVersion.Major}.{Request.ProtocolVersion.Minor}";
            CopyHeadersFrom(Request.Headers, Headers);
            SetBodyStream(Request);
        }

        /// <summary>
        /// Copy Request Headers from the <see cref="HttpListenerRequest"/> instance.
        /// </summary>
        /// <param name="Request"></param>
        internal static void CopyHeadersFrom(NameValueCollection RequestHeaders, IHttpHeaderCollection Headers)
        {
            foreach (var Each in RequestHeaders.AllKeys)
            {
                var Values = RequestHeaders.GetValues(Each);
                if (Values is null || Values.Length <= 1)
                {
                    var Value = RequestHeaders.Get(Each);
                    if (!string.IsNullOrWhiteSpace(Value))
                        Headers.Add(new KeyValuePair<string, string>(Each, Value));

                    continue;
                }

                foreach (var EachValue in Values)
                {
                    if (string.IsNullOrWhiteSpace(EachValue))
                        continue;

                    Headers.Add(new KeyValuePair<string, string>(Each, EachValue));
                }
            }
        }

        /// <summary>
        /// Set BodyStream from the <see cref="HttpListenerRequest"/> instance.
        /// </summary>
        /// <param name="Request"></param>
        private void SetBodyStream(HttpListenerRequest Request)
        {
            if (string.IsNullOrWhiteSpace(Request.Headers.Get("Content-Type")))
                InputStream = new MemoryStream(EMPTY_BODY, false);

            else
                InputStream = Request.InputStream;
        }

        /// <inheritdoc/>
        public IHttpContext Context { get; }

        /// <inheritdoc/>
        public string Method { get; set; }

        /// <inheritdoc/>
        public string PathString { get; set; }

        /// <inheritdoc/>
        public string QueryString { get; set; }

        /// <inheritdoc/>
        public string Protocol { get; }

        /// <inheritdoc/>
        public IHttpHeaderCollection Headers { get; } = new HttpHeaderCollection();
        
        /// <inheritdoc/>
        public Stream InputStream { get; set; }
    }
}
