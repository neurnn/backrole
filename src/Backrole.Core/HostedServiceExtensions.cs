using Backrole.Core.Abstractions;
using Backrole.Core.Internals.Hosting;
using System;
using System.Collections.Generic;

namespace Backrole.Core
{
    public static class HostedServiceExtensions
    {
        private static readonly object KEY = new object();

        /// <summary>
        /// Gets the collection that contains type of hosted services.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        private static List<Type> GetHostedServiceTypeCollection(this IServiceCollection This)
        {
            if (!This.Properties.TryGetValue<List<Type>>(KEY, out var List))
            {
                This.Properties[KEY] = List = new List<Type>();
                This.AddSingleton<IHostedService, HostedService>();
            }

            return List;
        }

        /// <summary>
        /// Add the hosted service by the factory delegate.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddHostedService(this IServiceCollection This, Func<IServiceProvider, IHostedService> Factory)
        {
            This.GetHostedServiceTypeCollection();

            return This.Configure(Services =>
            {
                if (Services.GetRequiredService<IHostedService>() is HostedService HostedServices)
                    HostedServices.Push(Factory);
            });
        }

        /// <summary>
        /// Add the hosted service that is created by the service injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddHostedService<ServiceType>(this IServiceCollection This) where ServiceType : IHostedService
        {
            var Types = This.GetHostedServiceTypeCollection();
            if (!Types.Contains(typeof(ServiceType)))
                 Types.Add(typeof(ServiceType));

            return This.AddHostedService(Services => Services.GetRequiredService<IServiceInjector>().Create(typeof(ServiceType)) as IHostedService);
        }

        /// <summary>
        /// Add the hosted service as unique that is created by the service injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddHostedServiceUnique<ServiceType>(this IServiceCollection This) where ServiceType : IHostedService
        {
            var Types = This.GetHostedServiceTypeCollection();
            if (!Types.Contains(typeof(ServiceType)))
                return This.AddHostedService<ServiceType>();

            return This;
        }
    }
}
