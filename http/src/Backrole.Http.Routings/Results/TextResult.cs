using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using Backrole.Http.Routings.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Results
{
    public class TextResult : IHttpResult
    {
        private static readonly byte[] EMPTY_BYTES = new byte[0];

        private string m_MimeType;
        private byte[] m_Content;

        /// <summary>
        /// Initialize a new <see cref="TextResult"/> instance as String content.
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="MimeType"></param>
        /// <param name="Encoding"></param>
        public TextResult(string Text, string MimeType = "text/html", Encoding Encoding = null)
        {
            if (Encoding is null)
                Encoding = Encoding.UTF8;

            m_Content = Encoding.GetBytes(Text);
            m_MimeType = HttpRouterUtils.SetEncodingToMimeType(MimeType, Encoding);
        }

        /// <summary>
        /// Initialize a new <see cref="TextResult"/> instance as Binary content.
        /// </summary>
        /// <param name="Binary"></param>
        /// <param name="MimeType"></param>
        /// <param name="Encoding"></param>
        public TextResult(ArraySegment<byte> Binary, string MimeType = "text/html")
        {
            if (Binary.Array != null)
                m_Content = Binary.Offset != 0 && Binary.Count != Binary.Array.Length ? Binary.ToArray() : Binary.Array;

            else
                m_Content = EMPTY_BYTES;

            m_MimeType = MimeType;
        }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Http)
        {
            Http.Response.Headers.Set("Content-Type", m_MimeType);
            Http.Response.OutputStream = new MemoryStream(m_Content, false);
            return Task.CompletedTask;
        }
    }
}
