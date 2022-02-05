using Backrole.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backrole.Http.QueryStrings
{
    /// <summary>
    /// Query String.
    /// </summary>
    public class QueryString
    {
        private IHttpRequest m_Request;
        private string m_QueryString;

        private Dictionary<string, string> m_KeyValues = new();

        /// <summary>
        /// Initialize a new <see cref="QueryString"/> instance.
        /// </summary>
        /// <param name="Request"></param>
        public QueryString(IHttpRequest Request) 
            => m_Request = Request;

        /// <summary>
        /// Prepare <see cref="m_KeyValues"/> from <see cref="IHttpRequest"/>.
        /// </summary>
        /// <returns></returns>
        private QueryString Prepare()
        {
            if (m_Request.QueryString != m_QueryString)
            {
                var KeyValues = m_Request.QueryString
                    .Split('&', StringSplitOptions.RemoveEmptyEntries)
                    .Select(X => X.Split('=', 2, StringSplitOptions.None));

                m_KeyValues.Clear();
                m_QueryString = m_Request.QueryString;

                foreach (var Each in KeyValues)
                {
                    if (Each.Length != 2)
                        continue;

                    var Key = Each.First();
                    var Value = Each.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(Key))
                        continue;

                    m_KeyValues[Key] = Uri.UnescapeDataString(Value ?? "");
                }
            }

            return this;
        }

        /// <summary>
        /// Apply changes to the query string of the <see cref="IHttpRequest"/>.
        /// </summary>
        /// <returns></returns>
        private QueryString Apply()
        {
            m_Request.QueryString = m_QueryString = string.Join('&',
                m_KeyValues.Select(X => $"{X.Key}={Uri.EscapeDataString(X.Value)}"));

            return this;
        }

        /// <summary>
        /// Gets or sets a value for the <paramref name="Key"/>
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string this[string Key]
        {
            get => Get(Key);
            set => Set(Key, value);
        }

        /// <summary>
        /// Gets all keys of the <see cref="QueryString"/>.
        /// </summary>
        public IEnumerable<string> Keys => Prepare().m_KeyValues.Keys;

        /// <summary>
        /// Test whether the key is defined on the <see cref="QueryString"/> or not.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public bool Contains(string Key) => Prepare().m_KeyValues.ContainsKey(Key);

        /// <summary>
        /// Sets a value for the key.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public QueryString Set(string Key, string Value)
        {
            Prepare()
                .m_KeyValues[Key] = Value;
            
            Apply();
            return this;
        }

        /// <summary>
        /// Gets a value by the key.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public string Get(string Key, string Default = null)
        {
            Prepare().m_KeyValues.TryGetValue(Key, out var Value);
            return Value ?? Default;
        }
    }
}
