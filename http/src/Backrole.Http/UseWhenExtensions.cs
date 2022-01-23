using Backrole.Http.Abstractions;
using Backrole.Http.Internals.Verbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http
{
    public static class UseWhenExtensions
    {
        /// <summary>
        /// Branches the middleware pipeline by the <paramref name="When"/> delegate.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="When"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static IHttpApplicationBuilder UseWhen(this IHttpApplicationBuilder This,
            Func<IHttpContext, Task<bool>> When, Action<IHttpMiddlewareBuilder> Body)
        {
            var Builder = new UseWhen(This).When(When);
            Body?.Invoke(Builder.Builder);
            return This.Use(Builder.Build());
        }

        /// <summary>
        /// Branches the middleware pipeline by the <paramref name="When"/> delegate.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="When"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static IHttpApplicationBuilder UseWhen(this IHttpApplicationBuilder This,
            Func<IHttpContext, bool> When, Action<IHttpMiddlewareBuilder> Body)
        {
            var Builder = new UseWhen(This).When(When);
            Body?.Invoke(Builder.Builder);
            return This.Use(Builder.Build());
        }
    }
}
