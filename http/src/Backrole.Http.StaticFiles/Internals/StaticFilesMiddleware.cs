using Backrole.Http.Abstractions;
using Backrole.Http.StaticFiles.Internals.Streams;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.StaticFiles.Internals
{
    internal class StaticFilesMiddleware
    {
        private StaticFilesOptions m_Options;

        /// <summary>
        /// Initialize a new <see cref="StaticFilesMiddleware"/> instance.
        /// </summary>
        /// <param name="Options"></param>
        public StaticFilesMiddleware(StaticFilesOptions Options)
            => m_Options = Options;

        /// <summary>
        /// Provides Static Files to browser.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="Next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(IHttpContext Http, Func<Task> Next)
        {
            var File = await m_Options.Translate(Http.Request);
            if (File != null && File.Exists)
            {
                Http.Response.Status = 200;
                Http.Response.StatusPhrase = null;

                await m_Options.PrependAsync(Http, File);
                var ETag = $"\"{MakeETag($"Backrole: {File.FullName}, {File.Length}, {File.LastWriteTime.ToString("r")}")}\"";

                Http.Response.Headers.Set("ETag", ETag);
                Http.Response.Headers.Set("Last-Modified", $"{File.LastWriteTimeUtc.ToString("r")}");

                if (!IfMatch(Http, ETag))
                {
                    Http.Response.Status = 412;
                    return;
                }

                if (IfModifiedSince(Http, File) || IfNoneMatch(Http, ETag))
                    SetOutputStream(Http, File);

                else
                {
                    Http.Response.Status = 304;
                }

                return;
            }

            await Next();
        }

        /// <summary>
        /// Test the "If-Modified-Since" header.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="File"></param>
        /// <returns></returns>
        private static bool IfModifiedSince(IHttpContext Http, FileInfo File)
        {
            var LastModified = Http.Request.Headers.GetValue("If-Modified-Since");
            if (LastModified != null)
            {
                if (DateTime.TryParseExact(LastModified, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out var Value))
                    return long.Parse(Value.ToString("yyyyMMddHHmmss")) < long.Parse(File.LastWriteTimeUtc.ToString("yyyyMMddHHmmss"));

                return true;
            }

            return LastModified == null;
        }

        /// <summary>
        /// Test the "If-Match" header.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="ETag"></param>
        /// <returns></returns>
        private static bool IfMatch(IHttpContext Http, string ETag)
        {
            var IfMatch = Http.Request.Headers.GetValue("If-Match");
            if (IfMatch != null)
            {
                return IfMatch.Trim() == ETag;
            }

            return true;
        }

        /// <summary>
        /// Test the "If-None-Match" header.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="ETag"></param>
        /// <returns></returns>
        private static bool IfNoneMatch(IHttpContext Http, string ETag)
        {
            var IfNoneMatch = Http.Request.Headers.GetValue("If-None-Match");
            if (IfNoneMatch != null)
            {
                return IfNoneMatch.Trim() != ETag;
            }

            return true;
        }

        /// <summary>
        /// Set Output Stream to send the file.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="File"></param>
        private static void SetOutputStream(IHttpContext Http, FileInfo File)
        {
            var MimeType = File.Name.Split('.').LastOrDefault() ?? "";
            HttpMimeTypes.Table.TryGetValue(MimeType, out MimeType);

            var Stream = File.OpenRead();
            Http.Response.Headers.Set("Content-Type", MimeType ?? "application/octet-stream");

            if (!Stream.CanSeek)
            {
                Http.Response.Headers.Set("Accept-Ranges", "none");
                Http.Response.OutputStream = Stream;
            }

            else
            {
                Http.Response.Headers.Set("Accept-Ranges", "bytes");

                var Ranges = ParseRangeHeader(Http.Request.Headers.GetValue("Range"));
                if (Ranges is null || Ranges.Length != 1 || Ranges[0].Start < 0)
                    Http.Response.OutputStream = Stream;

                else
                {
                    var Start = Ranges[0].Start;
                    var End = Ranges[0].End;

                    if (End < 0)
                        End = Stream.Length - 1;

                    Http.Response.Status = 206;
                    Http.Response.StatusPhrase = null;

                    Http.Response.Headers.Set("Content-Range", $"bytes {Start}-{End}/{Stream.Length}");
                    Http.Response.OutputStream = new RangeStream(Stream, Start, Math.Min(End + 1, Stream.Length));
                }
            }
        }

        private static Guid MakeETag(string String)
        {
            // 16 byte.
            using (var Md5 = MD5.Create())
                return new Guid(Md5.ComputeHash(Encoding.UTF8.GetBytes(String)));
        }

        /// <summary>
        /// Parse the range header.
        /// </summary>
        /// <param name="Range"></param>
        /// <returns></returns>
        private static (long Start, long End)[] ParseRangeHeader(string Range)
        {
            if (Range is null)
                return null;

            return Range
                .Split('=').SelectMany(X => X.Split(','))
                .Select(X =>
                {
                    var SE = X.Split('-').Select(Y =>
                    {
                        if (long.TryParse(Y, out var Z))
                            return Z;

                        return -1;
                    });

                    var S = SE.First();
                    var E = SE.Skip(1).FirstOrDefault();

                    if (SE.Skip(1).Count() <= 0)
                        E = -1;

                    return (Start: S, End: E);
                })
                .Where(X => X.Start >= 0)
                .ToArray();
        }
    }
}
