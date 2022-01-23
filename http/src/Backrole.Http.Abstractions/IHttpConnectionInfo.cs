using System;

namespace Backrole.Http.Abstractions
{
    public interface IHttpConnectionInfo
    {
        /// <summary>
        /// Local Address.
        /// </summary>
        string LocalAddress { get; }

        /// <summary>
        /// Remote Address.
        /// </summary>
        string RemoteAddress { get; }
    }
}
