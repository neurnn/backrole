using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Results
{
    public class HeaderResult : IHttpResult
    {
        private KeyValuePair<string, string>[] m_KeyValues;
        private IHttpResult m_Content;
        private bool m_Append;

        /// <summary>
        /// Initialize a new <see cref="HeaderResult"/> instance.
        /// </summary>
        /// <param name="KeyValues"></param>
        /// <param name="Content"></param>
        /// <param name="Append">false to replace</param>
        public HeaderResult(
            IEnumerable<KeyValuePair<string, string>> KeyValues,
            IHttpResult Content = null, bool Append = true)
        {
            m_KeyValues = KeyValues.ToArray();
            m_Content = Content;
            m_Append = Append;
        }

        /// <summary>
        /// Initialize a new <see cref="HeaderResult"/> instance.
        /// </summary>
        /// <param name="SingleHeader"></param>
        /// <param name="Content"></param>
        /// <param name="Append">false to replace</param>
        public HeaderResult(
            KeyValuePair<string, string> SingleHeader,
            IHttpResult Content = null, bool Append = true)
            : this(new[] { SingleHeader }, Content, Append)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="HeaderResult"/> instance.
        /// </summary>
        /// <param name="SingleHeader"></param>
        /// <param name="Content"></param>
        /// <param name="Append">false to replace</param>
        public HeaderResult(
            string Key, string Value,
            IHttpResult Content = null, bool Append = true)
            : this(new KeyValuePair<string, string>(Key, Value), Content, Append)
        {
        }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Http)
        {
            if (m_Append)
            {
                foreach (var Each in m_KeyValues)
                    Http.Response.Headers.Add(Each);
            }

            else
            {
                foreach (var Each in m_KeyValues)
                    Http.Response.Headers.Set(Each.Key, Each.Value);
            }

            if (m_Content != null)
                return m_Content.InvokeAsync(Http);

            return Task.CompletedTask;
        }
    }
}
