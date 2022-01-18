using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using Backrole.Core.Internals.Fallbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Services
{
    internal class ServiceFailbacks
    {
        /// <summary>
        /// Configure the fallback services.
        /// </summary>
        /// <param name="Dictionary"></param>
        public static void Configure(IDictionary<Type, IServiceRegistration> Dictionary)
        {
            /* Adds the fallback implementations for the logger and the configuration. */
            Fallback<ILoggerFactory>(Dictionary, ServiceRegistrations.Singleton<ILoggerFactory, NullLoggerFactory>);
            Fallback<IConfiguration>(Dictionary, ServiceRegistrations.Singleton<IConfiguration, NullConfiguration>);

            /* Adds the wrappers for the options, logger, lazy. */
            Failback(Dictionary, typeof(IOptions<>), typeof(Options<>));
            Failback(Dictionary, typeof(ILogger<>), typeof(Logger<>));
            Failback(Dictionary, typeof(ILazy<>), typeof(Lazy<>));

            /* Adds the wrapper that invokes the GetService method asynchronously.*/
            SetTaskAsAsyncAccess(Dictionary);
        }

        /// <summary>
        /// Set the service registration information if the service doesn't exist.
        /// </summary>
        /// <param name="Dictionary"></param>
        /// <param name="Generic"></param>
        /// <param name="ImplGeneric"></param>
        private static void Failback(IDictionary<Type, IServiceRegistration> Dictionary, Type Generic, Type ImplGeneric)
        {
            if (!Dictionary.ContainsKey(Generic))
            {
                Dictionary[Generic] = ServiceRegistrations.Singleton(Generic,
                   (Services, RequestedType) => MakeGenericWrapper(Services, ImplGeneric, RequestedType));
            }
        }


        /// <summary>
        /// Set the service registration information if the service doesn't exist.
        /// </summary>
        /// <param name="Dictionary"></param>
        private static void SetTaskAsAsyncAccess(IDictionary<Type, IServiceRegistration> Dictionary)
        {
            if (!Dictionary.ContainsKey(typeof(Task<>)))
            {
                Dictionary[typeof(Task<>)] = ServiceRegistrations.Singleton(typeof(Task<>),
                   (Services, RequestedType) =>
                   {
                       var RealType = RequestedType.GetGenericArguments().First();
                       var Method = typeof(ServiceFailbacks).GetMethod(nameof(ToGenericTask))
                            .MakeGenericMethod(RealType);

                       return Method.Invoke(null, new[] { Task.Run(() => Services.GetService(RealType)) });
                   });
            }
        }

        /// <summary>
        /// Convert the object task to <see cref="Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Task"></param>
        /// <returns></returns>
        public static async Task<T> ToGenericTask<T>(Task<object> Task)
        {
            if ((await Task) is T _T) return _T;
            throw new InvalidCastException($"The task returned invalid instance. it should be instance of {typeof(T).FullName}.");
        }

        /// <summary>
        /// Make generic wrapper instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="GenericType"></param>
        /// <param name="RequestedType"></param>
        /// <returns></returns>
        private static object MakeGenericWrapper(IServiceProvider Services, Type GenericType, Type RequestedType)
        {
            var RealType = GenericType.MakeGenericType(
                RequestedType.GetGenericArguments().FirstOrDefault());

            if (Services.GetService(typeof(IServiceInjector)) is IServiceInjector Injector)
                return Injector.Create(RealType);

            throw new InvalidOperationException("No service injector instance exists.");
        }

        /// <summary>
        /// Set the service registration information if the service doesn't exist.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="Dictionary"></param>
        /// <param name="Registration"></param>
        private static void Fallback<ServiceType>(IDictionary<Type, IServiceRegistration> Dictionary, Func<IServiceRegistration> Registration)
        {
            if (Dictionary.ContainsKey(typeof(ServiceType)))
                return;

            Dictionary[typeof(ServiceType)] = Registration();
        }

    }
}
