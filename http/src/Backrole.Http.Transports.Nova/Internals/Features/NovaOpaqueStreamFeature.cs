using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Features
{
    internal class NovaOpaqueStreamFeature : IHttpOpaqueStreamFeature
    {
        private INovaStream m_Stream;
        private IHttpContext m_Http;

        /// <summary>
        /// Initialize a new <see cref="NovaOpaqueStreamFeature"/> instance.
        /// </summary>
        /// <param name="Http"></param>
        public NovaOpaqueStreamFeature(IHttpContext Http)
        {
            m_Http = Http;
            m_Stream = Http.Properties.GetValue<INovaStream>(typeof(INovaStream))
                ?? throw new InvalidOperationException("No INovaStream interface is configured on the http properties.");
        }

        /// <inheritdoc/>
        public bool CanDowngrade => true;

        /// <inheritdoc/>
        public Task<Stream> DowngradeAsync()
        {
            var NovaHttp = m_Http.Properties.GetValue<NovaHttpContext>(typeof(NovaHttpContext));

            return m_Stream.UpgradeToOpaqueStreamAsync(NovaHttp);
        }
    }
}
