using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using Backrole.Http.Routings.Internals.Builders;
using Backrole.Http.Routings.Internals.Mappers;
using System;
using System.Linq;
using System.Reflection;

namespace Backrole.Http.Routings
{
    public static class HttpRoutingExtensions
    {
        /// <summary>
        /// Adds a router for mapping the HTTP endpoints to the application.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        public static IHttpApplicationBuilder UseRouter(this IHttpApplicationBuilder This, Action<IHttpRouterBuilder> Delegate)
        {
            var Builder = new HttpRouterBuilder(This.Properties);
            Delegate?.Invoke(Builder);

            var Router = Builder.Build();
            return This.Use(async (Http, Next) =>
            {
                var Endpoint = await Router.RouteAsync(Http);
                if (Endpoint != null)
                    await Endpoint.InvokeAsync(Http);

                else
                {
                    /* Reset the routing state to support multiple router stacks. */
                    Http.Properties.Remove(typeof(IHttpRouterState));
                    Http.Properties.Remove(typeof(IHttpRouterContext));

                    /* And reset the response status code. */
                    Http.Response.Status = 404;
                    Http.Response.StatusPhrase = null;
                    await Next();
                }
            });
        }

        /// <summary>
        /// Maps a <paramref name="TargetType"/> on the router that declares <see cref="HttpRouteAttribute"/> marked methods.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHttpRouterBuilder Map(this IHttpRouterBuilder This, Type TargetType)
        {
            ClassMappingData.GetMappingData(This, TargetType).ApplyTo(This);
            return This;
        }

        /// <summary>
        /// Maps a <typeparamref name="TargetType"/> on the router that declares <see cref="HttpRouteAttribute"/> marked methods.
        /// </summary>
        /// <typeparam name="TargetType"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHttpRouterBuilder Map<TargetType>(this IHttpRouterBuilder This) => Map(This, typeof(TargetType));

        /// <summary>
        /// Map all types that defined on the assembly.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Assembly"></param>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public static IHttpRouterBuilder MapAssembly(this IHttpRouterBuilder This, Assembly Assembly, Func<Type, bool> Filter = null)
        {
            var TargetTypes = Assembly.GetTypes()
                .Where(X => X.GetCustomAttribute<HttpRouteAttribute>() != null)
                .Where(X => Filter is null || Filter(X));

            foreach (var TargetType in TargetTypes)
                This.Map(TargetType);

            return This;
        }
    }
}
