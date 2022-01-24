using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backrole.Http.Routings.Internals
{
    internal static class HttpRouterUtils
    {
        /// <summary>
        /// Normalize the path that is routable.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static string NormalizePath(string Path)
            => string.Join('/', SplitPathToNames(Path));

        /// <summary>
        /// Split the path to name enumerable.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitPathToNames(string Path)
        {
            var Names = Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var Stack = new List<string>();

            foreach(var Each in Names)
            {
                if (Each == ".")
                    continue;

                if (Each == "..")
                {
                    if (Stack.Count > 0)
                        Stack.RemoveAt(Stack.Count - 1);
                    continue;
                }

                Stack.Add(Uri.UnescapeDataString(Each));
            }

            return Stack;
        }

        /// <summary>
        /// Get state of the <see cref="IHttpRouter"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static HttpRouterState GetRouterState(this IHttpContext This)
            => This.Properties.GetValue(typeof(IHttpRouterState), () => new HttpRouterState(This));

        /// <summary>
        /// Get state of the <see cref="IHttpRouter"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static HttpRouterContext GetRouterContext(this IHttpContext This)
            => This.Properties.GetValue(typeof(IHttpRouterContext), () => new HttpRouterContext(This));
    }
}
