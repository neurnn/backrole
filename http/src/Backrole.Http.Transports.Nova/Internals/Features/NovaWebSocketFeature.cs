using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Transports.Nova.Internals.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Features
{
    internal class NovaWebSocketFeature : IHttpWebSocketFeature
    {
        private static readonly Task<WebSocket> NULL_SOCKET = Task.FromResult(null as WebSocket);
        private const string WSOCK_HEADER = "websocket";
        private const string WSOCK_VERSION = "13";

        private static ReadOnlySpan<byte> WSOCK_BASEKEY => new byte[]
        {
                (byte)'2', (byte)'5', (byte)'8', (byte)'E', (byte)'A', (byte)'F', (byte)'A', (byte)'5', (byte)'-',
                (byte)'E', (byte)'9', (byte)'1', (byte)'4', (byte)'-', (byte)'4', (byte)'7', (byte)'D', (byte)'A',
                (byte)'-', (byte)'9', (byte)'5', (byte)'C', (byte)'A', (byte)'-', (byte)'C', (byte)'5', (byte)'A',
                (byte)'B', (byte)'0', (byte)'D', (byte)'C', (byte)'8', (byte)'5', (byte)'B', (byte)'1', (byte)'1'
        };

        private bool? m_IsWebSocket;
        private string m_RequestKey;
        private string[] m_Subprotocols;

        private IHttpOpaqueStreamFeature m_Opaque;
        private IHttpContext m_Http;

        /// <summary>
        /// Initialize a new <see cref="NovaWebSocketFeature"/> instance.
        /// </summary>
        /// <param name="Http"></param>
        public NovaWebSocketFeature(IHttpContext Http) 
            => m_Opaque = (m_Http = Http).Services.GetService<IHttpOpaqueStreamFeature>();

        /// <inheritdoc/>
        public IHttpRequest Request => m_Http.Request;

        /// <inheritdoc/>
        public IHttpServiceProvider Services => m_Http.Services;

        /// <inheritdoc/>
        public IHttpConnectionInfo Connection => m_Http.Connection;

        /// <inheritdoc/>
        public IEnumerable<string> Subprotocols
        {
            get
            {
                if (m_Subprotocols is null)
                {
                    var Subprotocol = m_Http.Request.Headers.GetValue("Sec-WebSocket-Protocol", null);
                    if (string.IsNullOrWhiteSpace(Subprotocol))
                        m_Subprotocols = new string[0];

                    else
                    {
                        m_Subprotocols = Subprotocol.Split(',').Select(X => X.Trim())
                            .Where(X => !string.IsNullOrWhiteSpace(X))
                            .ToArray();
                    }
                }

                return m_Subprotocols;
            }
        }

        /// <inheritdoc/>
        public bool CanUpgrade
        {
            get
            {
                if (m_IsWebSocket != null)
                    return m_IsWebSocket.Value;

                var Upgrade = m_Http.Request.Headers.GetValue("Upgrade");
                if (Upgrade != "websocket")
                    return (m_IsWebSocket = false).Value;

                return (m_IsWebSocket = CheckSupports()).Value;
            }
        }

        /// <inheritdoc/>
        public async Task<WebSocket> UpgradeAsync(string Subprotocol = null)
        {
            if (!CanUpgrade)
                throw new InvalidOperationException("The request isn't a WebSocket request.");

            try
            {
                while (true)
                {
                    //if ((string.IsNullOrWhiteSpace(Subprotocol) && Subprotocols.Count() > 0) ||
                    //   (!string.IsNullOrWhiteSpace(Subprotocol) && !Subprotocols.Contains(Subprotocol)))
                    //{
                    //    m_Http.Response.Status = 400; // Bad Request.
                    //    break;
                    //}

                    if (m_Opaque is null)
                    {
                        m_Http.Response.Status = 501; // Not Implemented
                        break;
                    }

                    m_Http.Response.Status = 101;

                    SetResponseHeaders(m_Http.Response, Subprotocol);

                    /* Upgrade the opaque stream to WebSocket. */
                    var Succeed = false;
                    var Stream = null as Stream;
                    try
                    {
                        var Socket = WebSocket.CreateFromStream(
                            Stream = await m_Opaque.DowngradeAsync(),
                            true, Subprotocol, TimeSpan.FromSeconds(5));

                        Succeed = true;
                        return Socket;
                    }
                    finally
                    {
                        if (!Succeed && Stream != null)
                        {
                            await Stream.DisposeAsync();
                        }
                    }
                }
            }

            catch
            {
                m_Http.Response.Status = 500; // Internal Server Error.
            }

            throw new InvalidOperationException("Failed to handshake the websocket.");
        }

        /// <summary>
        /// Set WebSocket upgrade response headers.
        /// </summary>
        /// <param name="Response"></param>
        /// <param name="Subprotocol"></param>
        private void SetResponseHeaders(IHttpResponse Response, string Subprotocol)
        {
            Response.Headers.Set("Connection", "upgrade");
            Response.Headers.Set("Upgrade", WSOCK_HEADER);
            Response.Headers.Set("Sec-WebSocket-Accept", MakeAcceptKey());

            if (!string.IsNullOrWhiteSpace(Subprotocol))
                Response.Headers.Set("Sec-WebSocket-Protocol", Subprotocol);
        }

        /// <summary>
        /// Make Accept Key.
        /// </summary>
        /// <returns></returns>
        private string MakeAcceptKey()
        {
            Span<byte> MergedBytes = stackalloc byte[60];
            Encoding.UTF8.GetBytes(m_RequestKey, MergedBytes);
            WSOCK_BASEKEY.CopyTo(MergedBytes[24..]);

            Span<byte> HashedBytes = stackalloc byte[20];
            if (SHA1.HashData(MergedBytes, HashedBytes) != 20)
                throw new InvalidOperationException("Could'nt compute the hash for creating an accept-key.");

            return Convert.ToBase64String(HashedBytes);
        }

        /// <summary>
        /// Check the websocket feature supported or not.
        /// </summary>
        /// <returns></returns>
        private bool CheckSupports()
        {
            if (m_Opaque is null || !m_Opaque.CanDowngrade)
                return false;

            var Request = m_Http.Request;
            var Headers = m_Http.Request.Headers;

            if (!Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) ||
                !TestExpectedHeaders(Request, "Sec-WebSocket-Version", WSOCK_VERSION) ||
                !TestExpectedHeaders(Request, "Connection", "upgrade") ||
                !TestExpectedHeaders(Request, "Upgrade", WSOCK_HEADER) ||
                string.IsNullOrWhiteSpace(Headers.GetValue("Sec-WebSocket-Key")))
                return false;

            Span<byte> Temp = stackalloc byte[16]; m_RequestKey = Headers.GetValue("Sec-WebSocket-Key");
            return Convert.TryFromBase64String(m_RequestKey, Temp, out var Written) && Written == 16;
        }

        /// <summary>
        /// Test expected headers presents.
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Header"></param>
        /// <param name="Expects"></param>
        /// <param name="Longrep"></param>
        /// <returns></returns>
        private bool TestExpectedHeaders(IHttpRequest Request, string Header, string Expects, string Longrep = null)
        {
            var Headers = m_Http.Request.Headers;
            var Values = (Headers.GetValue(Header) ?? "").Trim().Split(',')
                .Where(X => !string.IsNullOrWhiteSpace(X)).ToArray();

            if (Values is null || Values.Length <= 0)
                return false;

            if (Values.Count(X => X.Equals(Expects, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                if (Values.Length == 1)
                    Headers.Set("Sec-WebSocket-Protocol", Longrep ?? Expects);

                return true;
            }

            return false;
        }
    }
}
