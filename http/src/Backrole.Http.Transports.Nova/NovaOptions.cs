using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Http1;
using System;
using System.Net;

namespace Backrole.Http.Transports.Nova
{
    /// <summary>
    /// Nova Options.
    /// </summary>
    public sealed class NovaOptions
    {
        private Func<IHttpServiceProvider, NovaOptions, INovaStreamListener> m_Factory = DefaultFactory;

        /// <summary>
        /// Listen Mode (default: <see cref="NovaListenMode.Http1AndHttp2"/>)
        /// </summary>
        public NovaListenMode ListenMode { get; set; } = NovaListenMode.Http1;

        /// <summary>
        /// Local Address to listen.
        /// </summary>
        public IPAddress LocalAddress { get; set; } = IPAddress.Any;

        /// <summary>
        /// Local Port to listen (default: 5000).
        /// </summary>
        public int LocalPort { get; set; } = 5000;

        /// <summary>
        /// Backlog pendings (default: 20).
        /// </summary>
        public int Backlogs { get; set; } = 20;

        /// <summary>
        /// Disconnect the <see cref="INovaStream"/> if no data comming until specified timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Set the factory delegate to create an <see cref="INovaStreamListener"/> instance.
        /// </summary>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public NovaOptions Use(Func<IHttpServiceProvider, NovaOptions, INovaStreamListener> Factory)
        {
            m_Factory = Factory ?? DefaultFactory;
            return this;
        }

        /// <summary>
        /// Create a default <see cref="INovaStreamListener"/> instance using options.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Options"></param>
        /// <returns></returns>
        private static INovaStreamListener DefaultFactory(IHttpServiceProvider Services, NovaOptions Options)
        {
            switch (Options.ListenMode)
            {
                case NovaListenMode.Http1:
                    return new NovaHttpStreamListener(Options);

                case NovaListenMode.Http1AndHttp2:
                case NovaListenMode.Http2:
                    throw new NotImplementedException();
            }

            return null;
        }

        /// <summary>
        /// Create an <see cref="INovaStreamListener"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <returns></returns>
        internal INovaStreamListener Create(IHttpServiceProvider Services) => m_Factory(Services, this);
    }
}
