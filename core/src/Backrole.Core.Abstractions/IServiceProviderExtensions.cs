using System;

namespace Backrole.Core.Abstractions
{
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Get the service by its type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="Services"></param>
        /// <returns></returns>
        public static TService GetService<TService>(this IServiceProvider Services)
        {
            if (Services.GetService(typeof(TService)) is TService Instance)
                return Instance;

            return default;
        }

        /// <summary>
        /// Get the service by its type and throws <see cref="NotSupportedException"/> if no instance found.
        /// </summary>
        /// <param name="Services"></param>
        /// <returns></returns>
        public static object GetRequiredService(this IServiceProvider Services, Type ServiceType)
        {
            return Services.GetService(ServiceType) ??
                throw new NotSupportedException($"No {ServiceType.FullName} found.");
        }

        /// <summary>
        /// Get the service by its type and throws <see cref="NotSupportedException"/> if no instance found.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="Services"></param>
        /// <returns></returns>
        public static TService GetRequiredService<TService>(this IServiceProvider Services)
        {
            if (Services.GetService(typeof(TService)) is TService Instance)
                return Instance;

            throw new NotSupportedException($"No {typeof(TService).FullName} found.");
        }

        /// <summary>
        /// Create a new <see cref="IServiceScope"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Overrides">Overrides the scope specific services.</param>
        /// <returns></returns>
        public static IServiceScope CreateScope(this IServiceProvider Services, Action<IServiceCollection> Overrides = null) 
            => Services.GetRequiredService<IServiceScopeFactory>().CreateScope(Overrides);

        /// <summary>
        /// Create a new <see cref="IServiceScope"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Overrides">Overrides the scope specific services.</param>
        /// <returns></returns>
        public static IServiceScope CreateScope(this IServiceProvider Services, IServiceProperties Properties, Action<IServiceCollection> Overrides)
            => Services.GetRequiredService<IServiceScopeFactory>().CreateScope(Properties, Overrides);
    }
}
