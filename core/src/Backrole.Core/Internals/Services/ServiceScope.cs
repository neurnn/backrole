using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Services
{
    internal partial class ServiceScope : IServiceScope, IServiceScopeFactory, IServiceProvider
    {
        private ServiceRepository m_Repository = new();
        private ServiceDisposables m_Disposables = new();
        private ServiceHardcordedProvider m_Hardcords;

        private ServiceCollectionView m_Services;
        private ServiceScope m_Upper;

        private List<Task> m_DisposeLocks = new ();
        private bool m_Disposed = false;

        private Action<IServiceProvider> m_Configure;

        /// <summary>
        /// Initialize a new <see cref="ServiceScope"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="RootScope"></param>
        private ServiceScope(IServiceCollection Services, bool RootScope)
        {
            ServiceProvider = new ServiceOpaqueProvider(this);
            m_Hardcords = new ServiceHardcordedProvider()
                .Set<IServiceScope>(this)
                .Set<IServiceScopeFactory>(this)
                .Set<IServiceProvider>(ServiceProvider)
                .Set<IServiceInjector>(new Injector(this));

            if (Services != null)
            {
                m_Services = new ServiceCollectionView(Services)
                    .Alter(Dictionary =>
                    {
                        if (!RootScope) return;
                        ServiceFailbacks.Configure(Dictionary);
                    });

                m_Configure = Services.ConfigureDelegate;
            }
        }

        /// <summary>
        /// Initialize a new <see cref="ServiceScope"/> using the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="Services"></param>
        public ServiceScope(IServiceCollection Services) : this(Services, true) 
            => InvokeConfigureDelegate();

        /// <summary>
        /// Initialize a new <see cref="ServiceScope"/> using the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="Services"></param>
        private ServiceScope(ServiceScope Upper, IServiceCollection Overrides) : this(Overrides, false)
        {
            m_Upper = Upper;
            InvokeConfigureDelegate();
        }

        /// <summary>
        /// Invokes the <see cref="m_Configure"/> delegate and make it to be null.
        /// </summary>
        private void InvokeConfigureDelegate()
        {
            Action<IServiceProvider> Delegate;

            lock(this)
            {
                Delegate = m_Configure;
                m_Configure = null;
            }

            Delegate?.Invoke(ServiceProvider);
        }

        /// <inheritdoc/>
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc/>
        public IServiceScope CreateScope(Action<IServiceCollection> Overrides = null)
        {
            if (Overrides is null)
                return new ServiceScope(this, null);

            var Services = new ServiceCollection();
            Overrides?.Invoke(Services);

            /* Child scope should be disposed if the parent is disposing.*/
            var Scope = new ServiceScope(this, Services);
            m_Disposables.Reserve(Scope);
            return new ServiceScope(this, Services);
        }

        /// <inheritdoc/>
        public IServiceScope CreateScope(IServiceProperties Properties, Action<IServiceCollection> Overrides)
        {
            if (Overrides is null)
                return new ServiceScope(this, null);

            var Services = new ServiceCollection(Properties);
            Overrides?.Invoke(Services);

            /* Child scope should be disposed if the parent is disposing.*/
            var Scope = new ServiceScope(this, Services);
            m_Disposables.Reserve(Scope);
            return Scope;
        }

        /// <summary>
        /// Find an <see cref="IServiceRegistration"/> from the service hierarchy.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Where"></param>
        /// <returns></returns>
        public IServiceRegistration Find(Type ServiceType, out ServiceScope Where)
        {
            var Registration = m_Services.Find(ServiceType);
            if (Registration is null && m_Upper != null)
            {
                return m_Upper.Find(ServiceType, out Where);
            }

            Where = this;
            return Registration;
        }

        /// <inheritdoc/>
        public object GetService(Type ServiceType)
        {
            TaskCompletionSource Tcs;
            lock (this)
            {
                if (m_Disposed)
                    return null;

                m_DisposeLocks.Add((Tcs = new()).Task);
            }

            try
            {
                var Instance = m_Hardcords.GetService(ServiceType);
                if (Instance is null && 
                   (Instance = FindServiceFromHierarchy(ServiceType, out var Owner, out var Registration)) is null &&
                    Registration != null)
                {
                    switch (Registration.Lifetime)
                    {
                        case ServiceLifetime.Singleton:
                            if (Owner != this)
                                Instance = Owner.GetSingletonService(Registration, ServiceType);

                            else
                                Instance = GetSingletonService(Registration, ServiceType);

                            break;

                        case ServiceLifetime.Hierarchial:
                            Instance = GetSingletonService(Registration, ServiceType);
                            break;

                        case ServiceLifetime.Scoped:
                            Instance = GetScopedService(Registration, ServiceType, true);
                            break;

                        case ServiceLifetime.Transient:
                            Instance = Registration.GetInstance(ServiceProvider, ServiceType);
                            m_Disposables.Reserve(Instance);
                            break;
                    }
                }

                return Instance;
            }

            finally
            {
                Tcs.TrySetResult();
                lock (this)
                    m_DisposeLocks.Remove(Tcs.Task);
            }
        }

        /// <summary>
        /// Get Service instance that is fit on the parameter information.
        /// </summary>
        /// <param name="ParameterInfo"></param>
        /// <returns></returns>
        public object GetService(ParameterInfo ParameterInfo) => GetService(ParameterInfo.ParameterType, ParameterInfo);

        /// <summary>
        /// Get Service instance that is fit on the field information.
        /// </summary>
        /// <param name="FieldInfo"></param>
        /// <returns></returns>
        public object GetService(FieldInfo FieldInfo) => GetService(FieldInfo.FieldType, FieldInfo);

        /// <summary>
        /// Get Service instance that is fit on the property information.
        /// </summary>
        /// <param name="PropertyInfo"></param>
        /// <returns></returns>
        public object GetService(PropertyInfo PropertyInfo) => GetService(PropertyInfo.PropertyType, PropertyInfo);

        /// <summary>
        /// Get Service instance that is fit on the TargetType and Attributes information.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="Attributes"></param>
        /// <returns></returns>
        public object GetService(Type TargetType, ICustomAttributeProvider Attributes)
        {
            TaskCompletionSource Tcs;
            lock (this)
            {
                if (m_Disposed)
                    return null;

                m_DisposeLocks.Add((Tcs = new()).Task);
            }

            try
            {
                var Instance = GetServiceFromExtensions(TargetType, Attributes);
                if (Instance != null) return Instance;

                if (Attributes
                    .GetCustomAttributes(typeof(ServiceInjectionAttribute), false)
                    .FirstOrDefault() is ServiceInjectionAttribute InjectionInfo &&
                    (Instance = GetService(TargetType, InjectionInfo)) != null)
                    return Instance;
            }

            finally
            {
                Tcs.TrySetResult();
                lock (this)
                    m_DisposeLocks.Remove(Tcs.Task);
            }

            return GetService(TargetType);
        }

        /// <summary>
        /// Get Service instance that is fit on the TargetType and Attributes information.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="Attributes"></param>
        /// <returns></returns>
        public object GetService(Type TargetType, ICustomAttributeProvider Attributes, ServiceInjectionAttribute InjectionInfo)
        {
            TaskCompletionSource Tcs;
            lock (this)
            {
                if (m_Disposed)
                    return null;

                m_DisposeLocks.Add((Tcs = new()).Task);
            }

            try
            {
                var Instance = GetServiceFromExtensions(TargetType, Attributes);
                if (Instance != null) return Instance;

                if (InjectionInfo != null && (Instance = GetService(TargetType, InjectionInfo)) != null)
                    return Instance;
            }

            finally
            {
                Tcs.TrySetResult();
                lock (this)
                    m_DisposeLocks.Remove(Tcs.Task);
            }

            return GetService(TargetType);
        }

        /// <summary>
        /// Get Service using the <paramref name="InjectionInfo"/>.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="InjectionInfo"></param>
        /// <returns></returns>
        private object GetService(Type TargetType, ServiceInjectionAttribute InjectionInfo) 
            => GetService(InjectionInfo.ServiceType ?? TargetType);

        /// <summary>
        /// Get Service from extension.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="Attributes"></param>
        /// <returns></returns>
        private object GetServiceFromExtensions(Type TargetType, ICustomAttributeProvider Attributes)
        {
            var Scope = this;
            while (Scope != null)
            {
                var Services = Scope.m_Services;
                if (Services != null)
                {
                    var Instance = Services.Extension.GetService(ServiceProvider, TargetType, Attributes);
                    if (Instance != null)
                        return Instance;
                }

                Scope = Scope.m_Upper;
            }

            return null;
        }

        /// <summary>
        /// Find the service from the scope hierarchy.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Scope"></param>
        /// <param name="Registration"></param>
        /// <returns></returns>
        private object FindServiceFromHierarchy(Type ServiceType, out ServiceScope Scope, out IServiceRegistration Registration)
        {
            Scope = this;

            while (Scope != null)
            {
                var Services = Scope.m_Services;
                if (Services != null)
                {
                    var Instance = Services.Extension.GetService(ServiceProvider, ServiceType);
                    if (Instance != null)
                    {
                        Registration = null;
                        return Instance;
                    }

                    if ((Registration = Services.Find(ServiceType)) != null)
                        return null;
                }

                Scope = Scope.m_Upper;
            }

            Registration = null;
            return null;
        }

        /// <summary>
        /// Get Service that is singleton.
        /// </summary>
        /// <param name="Registration"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        private object GetSingletonService(IServiceRegistration Registration, Type ServiceType)
        {
            var Task = m_Repository.Reserve(ServiceType, out var Tcs);
            if (Tcs != null)
                ResolveServiceInstance(Registration, ServiceType, Tcs);

            return Task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get Service that is scoped.
        /// </summary>
        /// <param name="Registration"></param>
        /// <param name="ServiceType"></param>
        /// <param name="CreateNew"></param>
        /// <returns></returns>
        private object GetScopedService(IServiceRegistration Registration, Type ServiceType, bool CreateNew = false)
        {
            TaskCompletionSource<object> Tcs = null;
            Task<object> Reservation;

            lock(m_Repository)
            {
                if ((Reservation = m_Repository.GetReserved(ServiceType)) is null)
                {
                    object Instance = null;
                    if (m_Upper != null && (Instance = m_Upper.GetScopedService(Registration, ServiceType, false)) != null)
                        return Instance;

                    if (CreateNew)
                        Reservation = m_Repository.Reserve(ServiceType, out Tcs);
                }
            }

            if (Tcs != null)
                ResolveServiceInstance(Registration, ServiceType, Tcs);

            if (Reservation != null)
                return Reservation.GetAwaiter().GetResult();

            return null;
        }

        /// <summary>
        /// Resolve the service instance for the <paramref name="Registration"/>.
        /// </summary>
        /// <param name="Registration"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Tcs"></param>
        private void ResolveServiceInstance(IServiceRegistration Registration, Type ServiceType, TaskCompletionSource<object> Tcs)
        {
            try
            {
                var Instance = Registration.GetInstance(ServiceProvider, ServiceType);
                if (!Registration.KeepAlive)
                    m_Disposables.Reserve(Instance);

                Tcs.TrySetResult(Instance);
            }

            finally { Tcs.TrySetResult(null); }
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            lock (this)
            {
                if (m_Disposed)
                    return;

                m_Disposed = true;
            }

            await WaitDisposeLocks();
            await m_Disposables.DisposeAsync();

            m_Repository.Clear();
        }

        /// <summary>
        /// Wait Dispose locks released.
        /// </summary>
        /// <returns></returns>
        private async Task WaitDisposeLocks()
        {
            var Queue = new Queue<Task>();
            while (true)
            {
                lock (this)
                {
                    foreach (var Each in m_DisposeLocks)
                        Queue.Enqueue(Each);

                    if (Queue.Count <= 0)
                        break;
                }

                while (Queue.TryDequeue(out var Lock))
                    await Lock;
            }
        }
    }
}
