using Backrole.Http.Abstractions;
using Backrole.Http.StaticFiles.Internals;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backrole.Http.StaticFiles
{
    public static class StaticFilesExtensions
    {
        private static readonly Task<bool> ASYNC_FALSE = Task.FromResult(false);
        private static readonly Task<bool> ASYNC_TRUE = Task.FromResult(true);

        /// <summary>
        /// Adds a StaticFiles provider middleware to the application.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Options"></param>
        /// <returns></returns>
        public static IHttpApplicationBuilder UseStaticFiles(this IHttpApplicationBuilder This, StaticFilesOptions Options = null)
            => This.Use((new StaticFilesMiddleware(Options ?? new StaticFilesOptions())).InvokeAsync);

        /// <summary>
        /// Adds a StaticFiles provider middleware to the application.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Options"></param>
        /// <returns></returns>
        public static IHttpApplicationBuilder UseStaticFiles(this IHttpApplicationBuilder This, Action<StaticFilesOptions> Delegate)
        {
            var Options = new StaticFilesOptions();
            Delegate?.Invoke(Options);

            return This.UseStaticFiles(Options);
        }

        /// <summary>
        /// Disallow the specified extension.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Extension"></param>
        /// <returns></returns>
        public static StaticFilesOptions DisallowExtensions(this StaticFilesOptions This, params string[] Extensions)
        {
            for (var i = 0; i < Extensions.Length; ++i)
                Extensions[i] = "." + Extensions[i].TrimStart('.');

            return This.DisallowPostfix(Extensions);
        }

        /// <summary>
        /// Disallow the specified prefixes.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Prefixes"></param>
        /// <returns></returns>
        public static StaticFilesOptions DisallowPrefix(this StaticFilesOptions This, params string[] Prefixes)
        {
            This.UseFilter((Http, File) =>
            {
                foreach (var Each in Prefixes)
                {
                    if (File.Name.StartsWith(Each, StringComparison.OrdinalIgnoreCase))
                        return ASYNC_FALSE;
                }

                return ASYNC_TRUE;
            });

            return This;
        }

        /// <summary>
        /// Disallow the specified postfixes.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Postfixes"></param>
        /// <returns></returns>
        public static StaticFilesOptions DisallowPostfix(this StaticFilesOptions This, params string[] Postfixes)
        {
            This.UseFilter((Http, File) =>
            {
                foreach (var Each in Postfixes)
                {
                    if (File.Name.EndsWith(Each, StringComparison.OrdinalIgnoreCase))
                        return ASYNC_FALSE;
                }

                return ASYNC_TRUE;
            });

            return This;
        }
    }
}
