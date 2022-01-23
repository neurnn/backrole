using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Abstractions.Defaults
{
    /// <summary>
    /// Basic implementation of the <see cref="IHttpHeaderCollection"/>.
    /// </summary>
    public class HttpHeaderCollection : List<KeyValuePair<string, string>>, IHttpHeaderCollection
    {
        /// <inheritdoc/>
        public int IndexOf(string Key) => FindIndex(X => X.Key.Equals(Key, StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc/>
        public int LastIndexOf(string Key) => FindLastIndex(X => X.Key.Equals(Key, StringComparison.OrdinalIgnoreCase));
    }
}
