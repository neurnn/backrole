using Backrole.Http.Abstractions;
using Backrole.Http.Abstractions.Defaults;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Models
{
    internal class NovaHttpRequest : IHttpRequest
    {
        /// <inheritdoc/>
        public IHttpContext Context { get; set; }

        /// <inheritdoc/>
        public string Method { get; set; }

        /// <inheritdoc/>
        public string PathString { get; set; }

        /// <inheritdoc/>
        public string QueryString { get; set; }

        /// <inheritdoc/>
        public string Protocol { get; set; }

        /// <inheritdoc/>
        public IHttpHeaderCollection Headers { get; } = new HttpHeaderCollection();

        /// <inheritdoc/>
        public Stream InputStream { get; set; }

        /// <inheritdoc/>
        public Stream AbsoluteBodyStream { get; set; }
    }
}
