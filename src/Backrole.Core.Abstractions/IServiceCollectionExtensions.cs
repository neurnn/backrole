using Backrole.Core.Abstractions.Defaults;
using System;

namespace Backrole.Core.Abstractions
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Try to add the <see cref="IServiceRegistration"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Registration"></param>
        /// <returns></returns>
        public static IServiceCollection TryAdd(this IServiceCollection This, IServiceRegistration Registration)
        {
            var Previous = This.FindLast(Registration.Type);
            if (Previous != null)
                return This;

            This.Add(Registration);
            return This;
        }

        /// <summary>
        /// Remove the <see cref="IServiceRegistration"/> instance from the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static bool Remove(this IServiceCollection This, Type ServiceType)
        {
            var Registration = This.Find(ServiceType);
            if (Registration != null)
                return This.Remove(Registration);

            return false;
        }

        /// <summary>
        /// Remove the <see cref="IServiceRegistration"/> instance from the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static bool RemoveLast(this IServiceCollection This, Type ServiceType)
        {
            var Registration = This.FindLast(ServiceType);
            if (Registration != null)
                return This.Remove(Registration);

            return false;
        }

        /// <summary>
        /// Remove the <see cref="IServiceRegistration"/> instance from the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static bool Remove(this IServiceCollection This, Type ServiceType, out IServiceRegistration Registration)
        {
            if ((Registration = This.Find(ServiceType)) != null)
                return This.Remove(Registration);

            return false;
        }

        /// <summary>
        /// Remove the <see cref="IServiceRegistration"/> instance from the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static bool RemoveLast(this IServiceCollection This, Type ServiceType, out IServiceRegistration Registration)
        {
            if ((Registration = This.FindLast(ServiceType)) != null)
                return This.Remove(Registration);

            return false;
        }

        /// <summary>
        /// Add an <see cref="IServiceRegistration"/> instance to <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Registration"></param>
        /// <returns></returns>
        private static IServiceCollection Emplace(this IServiceCollection This, IServiceRegistration Registration, bool RemovePrevious = true)
        {
            while (RemovePrevious)
            {
                if (This.Remove(Registration.Type))
                    continue;

                break;
            }

            This.Add(Registration);
            return This;
        }

        /// <summary>
        /// Find a service by its service type from front of the collection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="This"></param>
        /// <returns></returns>
        public static bool Find(this IServiceCollection This, Type ServiceType, out IServiceRegistration Registration) => (Registration = This.Find(ServiceType)) != null;

        /// <summary>
        /// Find a service by its service type from back of the collection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="This"></param>
        /// <returns></returns>
        public static bool FindLast(this IServiceCollection This, Type ServiceType, out IServiceRegistration Registration) => (Registration = This.FindLast(ServiceType)) != null;

        /// <summary>
        /// Find a service by its service type from front of the collection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static bool Find<ServiceType>(this IServiceCollection This, out IServiceRegistration Registration) => (Registration = This.Find<ServiceType>()) != null;

        /// <summary>
        /// Find a service by its service type from back of the collection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static bool FindLast<ServiceType>(this IServiceCollection This, out IServiceRegistration Registration) => (Registration = This.FindLast<ServiceType>()) != null;

        /// <summary>
        /// Find a service by its service type from front of the collection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceRegistration Find<ServiceType>(this IServiceCollection This) => This.Find(typeof(ServiceType));

        /// <summary>
        /// Find a service by its service type from back of the collection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceRegistration FindLast<ServiceType>(this IServiceCollection This) => This.FindLast(typeof(ServiceType));

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="KeepAlive"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection This, Type ServiceType, object Instance, bool KeepAlive = false)
            => This.Emplace(ServiceRegistrations.Singleton(ServiceType, Instance, KeepAlive));

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection This, Type ServiceType, Type ImplType = null)
            => This.Emplace(ServiceRegistrations.Singleton(ServiceType, ImplType));

        /// <summary>
        /// Add a Hierarchial service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static IServiceCollection AddHierarchial(this IServiceCollection This, Type ServiceType, Type ImplType = null)
            => This.Emplace(ServiceRegistrations.Hierarchial(ServiceType, ImplType));

        /// <summary>
        /// Add a Scoped service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection This, Type ServiceType, Type ImplType = null)
            => This.Emplace(ServiceRegistrations.Scoped(ServiceType, ImplType));

        /// <summary>
        /// Add a Transient service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection This, Type ServiceType, Type ImplType = null)
            => This.Emplace(ServiceRegistrations.Transient(ServiceType, ImplType));

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<ServiceType>(this IServiceCollection This)
            => This.Emplace(ServiceRegistrations.Singleton<ServiceType>());

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<ServiceType>(this IServiceCollection This, ServiceType Instance, bool KeepAlive = false)
            => This.Emplace(ServiceRegistrations.Singleton(typeof(ServiceType), Instance, KeepAlive));

        /// <summary>
        /// Add a Hierarchial service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddHierarchial<ServiceType>(this IServiceCollection This)
            => This.Emplace(ServiceRegistrations.Hierarchial<ServiceType>());

        /// <summary>
        /// Add a Scoped service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddScoped<ServiceType>(this IServiceCollection This)
            => This.Emplace(ServiceRegistrations.Scoped<ServiceType>());

        /// <summary>
        /// Add a Transient service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddTransient<ServiceType>(this IServiceCollection This)
            => This.Emplace(ServiceRegistrations.Transient<ServiceType>());

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<ServiceType, ImplType>(this IServiceCollection This)
            => This.Emplace(ServiceRegistrations.Singleton<ServiceType, ImplType>());

        /// <summary>
        /// Add a Hierarchial service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddHierarchial<ServiceType, ImplType>(this IServiceCollection This)
            => This.Emplace(ServiceRegistrations.Hierarchial<ServiceType, ImplType>());

        /// <summary>
        /// Add a Scoped service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddScoped<ServiceType, ImplType>(this IServiceCollection This)
            => This.Emplace(ServiceRegistrations.Scoped<ServiceType, ImplType>());

        /// <summary>
        /// Add a Transient service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddTransient<ServiceType, ImplType>(this IServiceCollection This)
            => This.Emplace(ServiceRegistrations.Transient<ServiceType, ImplType>());

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, object> Factory)
            => This.Emplace(ServiceRegistrations.Singleton(ServiceType, (Services, _) => Factory(Services)));

        /// <summary>
        /// Add a Hierarchial service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddHierarchial(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, object> Factory)
            => This.Emplace(ServiceRegistrations.Hierarchial(ServiceType, (Services, _) => Factory(Services)));

        /// <summary>
        /// Add a Scoped service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, object> Factory)
            => This.Emplace(ServiceRegistrations.Scoped(ServiceType, (Services, _) => Factory(Services)));

        /// <summary>
        /// Add a Transient service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, object> Factory)
            => This.Emplace(ServiceRegistrations.Transient(ServiceType, (Services, _) => Factory(Services)));

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<ServiceType>(this IServiceCollection This, Func<IServiceProvider, object> Factory)
            => This.Emplace(ServiceRegistrations.Singleton(typeof(ServiceType), (Services, _) => Factory(Services)));

        /// <summary>
        /// Add a Hierarchial service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddHierarchial<ServiceType>(this IServiceCollection This, Func<IServiceProvider, object> Factory)
            => This.Emplace(ServiceRegistrations.Hierarchial(typeof(ServiceType), (Services, _) => Factory(Services)));

        /// <summary>
        /// Add a Scoped service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddScoped<ServiceType>(this IServiceCollection This, Func<IServiceProvider, object> Factory)
            => This.Emplace(ServiceRegistrations.Scoped(typeof(ServiceType), (Services, _) => Factory(Services)));

        /// <summary>
        /// Add a Transient service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddTransient<ServiceType>(this IServiceCollection This, Func<IServiceProvider, object> Factory)
            => This.Emplace(ServiceRegistrations.Transient(typeof(ServiceType), (Services, _) => Factory(Services)));

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, Type, object> Factory)
            => This.Emplace(ServiceRegistrations.Singleton(ServiceType, Factory));

        /// <summary>
        /// Add a Hierarchial service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddHierarchial(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, Type, object> Factory)
            => This.Emplace(ServiceRegistrations.Hierarchial(ServiceType, Factory));

        /// <summary>
        /// Add a Scoped service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, Type, object> Factory)
            => This.Emplace(ServiceRegistrations.Scoped(ServiceType, Factory));

        /// <summary>
        /// Add a Transient service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, Type, object> Factory)
            => This.Emplace(ServiceRegistrations.Transient(ServiceType, Factory));

        /// <summary>
        /// Add a Singleton service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<ServiceType>(this IServiceCollection This, Func<IServiceProvider, Type, object> Factory)
            => This.Emplace(ServiceRegistrations.Singleton(typeof(ServiceType), Factory));

        /// <summary>
        /// Add a Hierarchial service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddHierarchial<ServiceType>(this IServiceCollection This, Func<IServiceProvider, Type, object> Factory)
            => This.Emplace(ServiceRegistrations.Hierarchial(typeof(ServiceType), Factory));

        /// <summary>
        /// Add a Scoped service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddScoped<ServiceType>(this IServiceCollection This, Func<IServiceProvider, Type, object> Factory)
            => This.Emplace(ServiceRegistrations.Scoped(typeof(ServiceType), Factory));

        /// <summary>
        /// Add a Transient service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddTransient<ServiceType>(this IServiceCollection This, Func<IServiceProvider, Type, object> Factory)
            => This.Emplace(ServiceRegistrations.Transient(typeof(ServiceType), Factory));
    }
}
