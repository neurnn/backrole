using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Internals.Mappers
{
    using MappingTupleList = List<(string Path, string Method, Func<IHttpContext, Task<IHttpRouterEndpoint>> Endpoint)>;
    internal class ClassMappingData
    {
        private const BindingFlags BF_PUBLIC = BindingFlags.Instance | BindingFlags.Public;
        private const BindingFlags BF_HIDDEN = BindingFlags.Instance | BindingFlags.NonPublic;

        private MappingTupleList m_Methods = new();
        private Type m_TargetType;

        /// <summary>
        /// Get Mapping Cache Dictionary.
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        private static Dictionary<Type, ClassMappingData> GetMappingCaches(IHttpRouterBuilder Builder)
            => Builder.Properties.GetValue(typeof(Dictionary<Type, ClassMappingData>), () => new Dictionary<Type, ClassMappingData>());

        /// <summary>
        /// Make tuple of <see cref="MethodInfo"/> and <see cref="HttpRouteAttribute"/>.
        /// </summary>
        /// <param name="Method"></param>
        /// <returns></returns>
        private static (MethodInfo Method, HttpRouteAttribute Route) MAKE_TUPLE(MethodInfo Method)
            => (Method, Method.GetCustomAttribute<HttpRouteAttribute>(true));

        /// <summary>
        /// Get Mapping Data for mapping the <paramref name="TargetType"/>.
        /// </summary>
        /// <param name="Builder"></param>
        /// <param name="TargetType"></param>
        /// <returns></returns>
        public static ClassMappingData GetMappingData(IHttpRouterBuilder Builder, Type TargetType)
        {
            var Caches = GetMappingCaches(Builder);
            if (Caches.TryGetValue(TargetType, out var Cache))
                return Cache;

            return Caches[TargetType] = new ClassMappingData(TargetType);
        }

        /// <summary>
        /// Initialize a new <see cref="ClassMappingData"/> instance.
        /// </summary>
        /// <param name="TargetType"></param>
        public ClassMappingData(Type TargetType)
        {
            var Methods = (m_TargetType = TargetType)
                .GetMethods(BF_PUBLIC).Select(MAKE_TUPLE).Where(X => X.Route != null)
                .Concat(TargetType.GetMethods(BF_HIDDEN).Select(MAKE_TUPLE).Where(X => X.Route != null));

            var BaseRoute = m_TargetType.GetCustomAttribute<HttpRouteAttribute>();
            var BasePath = (BaseRoute != null ? BaseRoute.Path : "").TrimEnd('/');

            foreach(var Pair in Methods)
            {
                var Path = (Pair.Route.Path ?? "/").TrimStart('/');
                var Method = Pair.Route.Method ?? "GET, POST";
                var Endpoint = new ClassMappedEndpoint(m_TargetType, Pair.Method);

                m_Methods.Add(($"{BasePath}/{Path}", Method, Endpoint.MakeFactory()));
            }
        }
        
        /// <summary>
        /// Apply the class mapping data to the given builder instance.
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public ClassMappingData ApplyTo(IHttpRouterBuilder Builder)
        {
            foreach(var Each in m_Methods)
            {
                var Methods = Each
                    .Method.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(X => X.Trim());

                foreach(var Method in Methods)
                    Builder.Map(Each.Path, X => X.On(Method, Each.Endpoint));
            }

            return this;
        }
    }
}
