using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.StaticFiles.Internals
{
    internal class HttpMimeTypes : Dictionary<string, string>
    {
        private static readonly char[] SEPARATOR = new char[] { ' ', '\t' };
        private HttpMimeTypes()
        {
            var Table = Resources.HttpMimeTypes.Split('\n')
                .Select(X => X.Trim(' ', '\t', '\r', '\n'))
                .Where(X => !string.IsNullOrWhiteSpace(X))
                .Select(X => X.Split(SEPARATOR, 2, StringSplitOptions.RemoveEmptyEntries)).Where(X => X.Length > 1)
                .Select(X => (Extension: X.First().TrimStart('.'), MimeType: X.LastOrDefault() ?? "application/octet-stream"));

            foreach (var Each in Table)
                this[Each.Extension] = Each.MimeType;
        }

        /// <summary>
        /// Status Code Table.
        /// </summary>
        public static HttpMimeTypes Table { get; } = new HttpMimeTypes();

        /// <summary>
        /// Set the encoding to the mime-type string.
        /// </summary>
        public static string SetCharset(string MimeType, Encoding Encoding)
        {
            if (Encoding != null)
            {
                /* Remove `charset=...` part from mime-type. */
                if (MimeType.Contains("charset=", StringComparison.OrdinalIgnoreCase))
                {
                    MimeType = string.Join("; ", MimeType.Split(';').Select(X => X.Trim())
                        .Where(X => X.StartsWith("charset=", StringComparison.OrdinalIgnoreCase)));
                }

                /* Then, append exact encoding. */
                MimeType = $"{MimeType.TrimEnd(' ', ';')}; charset={Encoding.WebName}";
            }

            return MimeType;
        }
    }
}
