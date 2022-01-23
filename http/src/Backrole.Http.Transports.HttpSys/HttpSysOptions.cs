using System;
using System.Collections.Generic;
using System.Net;

namespace Backrole.Http.Transports.HttpSys
{
    public sealed class HttpSysOptions
    {
        private List<Action<HttpListener>> m_Configs = new();

        /// <summary>
        /// Adds a delegate that configures <see cref="HttpListener"/> instance for the transport.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        public HttpSysOptions Configure(Action<HttpListener> Delegate)
        {
            m_Configs.Add(Delegate);
            return this;
        }

        /// <summary>
        /// Removes all prefix that the Http.Sys listens.
        /// </summary>
        /// <param name="Prefix"></param>
        /// <returns></returns>
        public HttpSysOptions Clear()
        {
            m_Configs.Add(Listener => Listener.Prefixes.Clear());
            return this;
        }

        /// <summary>
        /// Add a prefix that the Http.Sys listens.
        /// </summary>
        /// <param name="Prefix"></param>
        /// <returns></returns>
        public HttpSysOptions Add(string Prefix)
        {
            m_Configs.Add(Listener => Listener.Prefixes.Add(Prefix));
            return this;
        }

        /// <summary>
        /// Remove the prefix that the Http.Sys listens.
        /// </summary>
        /// <param name="Prefix"></param>
        /// <returns></returns>
        public HttpSysOptions Remove(string Prefix)
        {
            m_Configs.Add(Listener => Listener.Prefixes.Remove(Prefix));
            return this;
        }

        /// <summary>
        /// Create a new <see cref="HttpListener"/> instance.
        /// </summary>
        /// <returns></returns>
        internal HttpListener Create()
        {
            var New = new HttpListener();
            foreach (var Each in m_Configs)
                Each?.Invoke(New);

            return New;
        }
    }
}
