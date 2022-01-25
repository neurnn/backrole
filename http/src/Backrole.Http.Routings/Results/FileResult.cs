using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using Backrole.Http.Routings.Internals.Streams;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Results
{
    public class FileResult : IHttpResult
    {
        private Func<Task<Stream>> m_StreamFactory;
        private string m_MimeType;

        private DateTime m_ModifiedTime;

        /// <summary>
        /// Initialize a new <see cref="FileResult"/> instance.
        /// </summary>
        /// <param name="StreamFactory"></param>
        /// <param name="MimeType"></param>
        public FileResult(Func<Task<Stream>> StreamFactory, string MimeType, DateTime? ModifiedTime = null)
        {
            m_StreamFactory = StreamFactory;
            m_MimeType = MimeType;

            m_ModifiedTime = ModifiedTime.HasValue 
                ? ModifiedTime.Value : DateTime.Now;
        }

        /// <summary>
        /// Create the <see cref="FileResult"/> from the file info.
        /// The stream will be closed after transfered.
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="MimeType"></param>
        /// <returns></returns>
        public static FileResult FromFile(FileInfo Stream, string MimeType = "application/octet-stream", DateTime? ModifiedTime = null)
            => new FileResult(() => Task.FromResult<Stream>(Stream.OpenRead()), MimeType, ModifiedTime);

        /// <summary>
        /// Create the <see cref="FileResult"/> from the stream.
        /// The stream will be closed after transfered.
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="MimeType"></param>
        /// <returns></returns>
        public static FileResult FromStream(Stream Stream, string MimeType = "application/octet-stream", DateTime? ModifiedTime = null)
            => new FileResult(() => Task.FromResult(Stream), MimeType, ModifiedTime);

        /// <summary>
        /// Create the <see cref="FileResult"/> from the byte array.
        /// </summary>
        /// <param name="Segment"></param>
        /// <param name="MimeType"></param>
        /// <returns></returns>
        public static FileResult FromBytes(ArraySegment<byte> Segment, string MimeType = "application/octet-stream", DateTime? ModifiedTime = null)
            => new FileResult(() => Task.FromResult<Stream>(new MemoryStream(Segment.Array, Segment.Offset, Segment.Count, false)), MimeType, ModifiedTime);

        /// <inheritdoc/>
        public async Task InvokeAsync(IHttpContext Http)
        {
            var Stream = await m_StreamFactory();
            var ETag = $"\"{MakeETag($"Backrole: {m_ModifiedTime.ToString("r")}")}\"";

            Http.Response.Headers.Set("ETag", ETag);
            Http.Response.Headers.Set("Last-Modified", $"{m_ModifiedTime.ToString("r")}");
            Http.Response.Headers.Set("Content-Type", m_MimeType);

            Http.Response.Status = 200;
            Http.Response.StatusPhrase = null;

            if (!IfMatch(Http, ETag))
            {
                Http.Response.Status = 412;
                await Stream.DisposeAsync();
                return;
            }

            if (IfModifiedSince(Http, m_ModifiedTime) || IfNoneMatch(Http, ETag))
                SetOutputStream(Http, Stream);

            else
            {
                Http.Response.Status = 304;
                await Stream.DisposeAsync();
            }

        }

        /// <summary>
        /// Test the "If-Modified-Since" header.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="File"></param>
        /// <returns></returns>
        private static bool IfModifiedSince(IHttpContext Http, DateTime FileTime)
        {
            var LastModified = Http.Request.Headers.GetValue("If-Modified-Since");
            if (LastModified != null)
            {
                if (DateTime.TryParseExact(LastModified, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out var Value))
                    return long.Parse(Value.ToString("yyyyMMddHHmmss")) < long.Parse(FileTime.ToString("yyyyMMddHHmmss"));

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
        /// <param name="Stream"></param>
        private static void SetOutputStream(IHttpContext Http, Stream Stream)
        {
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
