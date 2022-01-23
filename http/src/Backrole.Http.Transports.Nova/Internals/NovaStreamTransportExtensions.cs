using Backrole.Http.Transports.Nova.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals
{
    internal static class NovaStreamTransportExtensions
    {
        private const byte LINE_FEED = (byte)'\n';

        /// <summary>
        /// Read a line from the <see cref="INovaStreamTransport"/> instance.
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="Encoding"></param>
        /// <returns></returns>
        public static async Task<string> ReadLineAsync(this INovaStreamTransport Transport, Encoding Encoding = null)
        {
            using (var Buffer = new MemoryStream())
            {
                var Temp = new byte[2048];
                while (true)
                {
                    var Length = await Transport.PeekAsync(Temp);
                    if (Length <= 0)
                        break;

                    var Index = Array.IndexOf(Temp, LINE_FEED, 0, Length);
                    if (Index >= 0)
                    {
                        Buffer.Write(Temp, 0, Index + 1);
                        Transport.AdvanceTo(Index + 1);

                        Buffer.Position = 0;

                        try { return (Encoding ?? Encoding.UTF8).GetString(Buffer.ToArray()); }
                        catch
                        {
                            break;
                        }
                    }

                    Buffer.Write(Temp, 0, Length);
                    Transport.AdvanceTo(Length);
                }

                return null;
            }
        }

        /// <summary>
        /// Read lines from the <see cref="INovaStreamTransport"/> instance.
        /// </summary>
        /// <param name="Transport"></param>
        /// <param name="BreakOnEmpty"></param>
        /// <returns></returns>
        public static async IAsyncEnumerator<string> ReadLines(this INovaStreamTransport Transport, Encoding Encoding = null, bool BreakOnEmpty = true)
        {
            var EmptyYet = true;
            while (true)
            {
                var Line = await Transport.ReadLineAsync(Encoding);
                if (Line is null)
                {
                    break;
                }

                if ((Line = Line.Trim(' ', '\t', '\r', '\n')).Length <= 0)
                {
                    if (EmptyYet)
                        continue;

                    if (BreakOnEmpty)
                        break;
                }

                EmptyYet = false;
                yield return Line;
            }
        }
    }
}
