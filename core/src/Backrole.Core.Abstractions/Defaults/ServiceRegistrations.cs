using System;

namespace Backrole.Core.Abstractions.Defaults
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class ServiceRegistrations
    {
        /// <summary>
        /// Try to get an instance if the registration is created by the <see cref="Singleton(Type, object, bool)"/>.
        /// </summary>
        /// <param name="Registration"></param>
        /// <param name="OutInstance"></param>
        /// <returns></returns>
        public static bool TryGet(this IServiceRegistration Registration, out object OutInstance)
        {
            if (Registration is FromInstance Singleton)
            {
                OutInstance = Singleton.Instance;
                return true;
            }

            OutInstance = null;
            return false;
        }

        /// <summary>
        /// Try to get an instance if the registration is created by the <see cref="Singleton(Type, object, bool)"/> and the instance is assignable to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="Registration"></param>
        /// <param name="OutValue"></param>
        /// <returns></returns>
        public static bool TryGet<T>(this IServiceRegistration Registration, out T OutValue)
        {
            if (Registration is FromInstance Singleton &&
                Singleton.Instance is T Value)
            {
                OutValue = Value;
                return true;
            }

            OutValue = default;
            return false;
        }

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Instance"></param>
        /// <param name="KeepAlive"></param>
        /// <returns></returns>
        public static IServiceRegistration Singleton(Type ServiceType, object Instance, bool KeepAlive = false)
            => new FromInstance(ServiceType, Instance, KeepAlive);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceRegistration Singleton(Type ServiceType, Func<IServiceProvider, Type, object> Factory)
            => new FromFactory(ServiceType, ServiceLifetime.Singleton, Factory);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Hierarchial"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceRegistration Hierarchial(Type ServiceType, Func<IServiceProvider, Type, object> Factory)
            => new FromFactory(ServiceType, ServiceLifetime.Hierarchial, Factory);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Scoped"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceRegistration Scoped(Type ServiceType, Func<IServiceProvider, Type, object> Factory)
            => new FromFactory(ServiceType, ServiceLifetime.Scoped, Factory);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Transient"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceRegistration Transient(Type ServiceType, Func<IServiceProvider, Type, object> Factory)
            => new FromFactory(ServiceType, ServiceLifetime.Transient, Factory);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> by the depedency injection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Lifetime"></param>
        /// <returns></returns>
        private static IServiceRegistration ByInjection(Type ServiceType, ServiceLifetime Lifetime, Type ImplType)
        {
            if (ImplType.IsAbstract)
                throw new InvalidOperationException($"{ImplType.FullName} is abstract, so it can not be instantiated.");

            if (!ServiceType.IsAssignableFrom(ImplType))
                throw new InvalidOperationException($"{ImplType.FullName} isn't compatible with {ServiceType.FullName}.");

            return new FromFactory(ServiceType, Lifetime, (Services, _) =>
            {
                if (Services.GetService(typeof(IServiceInjector)) is IServiceInjector Injector)
                    return Injector.Create(ImplType);

                throw new InvalidOperationException("No service injector exists.");
            });
        }

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Singleton"/> and with the dependency injection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplType"></param>
        /// <returns></returns>
        public static IServiceRegistration Singleton(Type ServiceType, Type ImplType = null)
            => ByInjection(ServiceType, ServiceLifetime.Singleton, ImplType ?? ServiceType);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Hierarchial"/> and with the dependency injection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplType"></param>
        /// <returns></returns>
        public static IServiceRegistration Hierarchial(Type ServiceType, Type ImplType = null)
            => ByInjection(ServiceType, ServiceLifetime.Hierarchial, ImplType ?? ServiceType);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Scoped"/> and with the dependency injection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplType"></param>
        /// <returns></returns>
        public static IServiceRegistration Scoped(Type ServiceType, Type ImplType = null)
            => ByInjection(ServiceType, ServiceLifetime.Scoped, ImplType ?? ServiceType);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Transient"/> and with the dependency injection.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplType"></param>
        /// <returns></returns>
        public static IServiceRegistration Transient(Type ServiceType, Type ImplType = null)
            => ByInjection(ServiceType, ServiceLifetime.Transient, ImplType ?? ServiceType);

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Singleton"/> and with the dependency injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <typeparam name="ImplType"></typeparam>
        /// <returns></returns>
        public static IServiceRegistration Singleton<ServiceType, ImplType>() => Singleton(typeof(ServiceType), typeof(ImplType));

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Hierarchial"/> and with the dependency injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <typeparam name="ImplType"></typeparam>
        /// <returns></returns>
        public static IServiceRegistration Hierarchial<ServiceType, ImplType>() => Hierarchial(typeof(ServiceType), typeof(ImplType));

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Scoped"/> and with the dependency injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <typeparam name="ImplType"></typeparam>
        /// <returns></returns>
        public static IServiceRegistration Scoped<ServiceType, ImplType>() => Scoped(typeof(ServiceType), typeof(ImplType));

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Transient"/> and with the dependency injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <typeparam name="ImplType"></typeparam>
        /// <returns></returns>
        public static IServiceRegistration Transient<ServiceType, ImplType>() => Transient(typeof(ServiceType), typeof(ImplType));

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Singleton"/> and with the dependency injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceRegistration Singleton<ServiceType>() => Singleton(typeof(ServiceType));

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Hierarchial"/> and with the dependency injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceRegistration Hierarchial<ServiceType>() => Hierarchial(typeof(ServiceType));

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Scoped"/> and with the dependency injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceRegistration Scoped<ServiceType>() => Scoped(typeof(ServiceType));

        /// <summary>
        /// Creates an <see cref="IServiceRegistration"/> instance that represents <see cref="ServiceLifetime.Transient"/> and with the dependency injection.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <returns></returns>
        public static IServiceRegistration Transient<ServiceType>() => Transient(typeof(ServiceType));
    }
}
