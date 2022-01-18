using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using System;

namespace Backrole.Core.Builders
{
    /// <summary>
    /// Base implementation of the <see cref="IContainerBuilder"/> that uses dependency injection to create container.
    /// </summary>
    public abstract class ContainerBuilder<TContainer> : IContainerBuilder where TContainer : class, IContainer
    {
        /// <summary>
        /// Initialize a new <see cref="ContainerBuilder{TContainer}"/> instance.
        /// </summary>
        /// <param name="HostServices"></param>
        public ContainerBuilder(IServiceProvider HostServices) => this.HostServices = HostServices;

        /// <inheritdoc/>
        public IServiceProvider HostServices { get; }

        /// <inheritdoc/>
        public IServiceProperties Properties { get; } = new ServiceProperties();

        /// <summary>
        /// Build a <typeparamref name="TContainer"/> instance.
        /// </summary>
        /// <returns></returns>
        public TContainer Build() => Build(HostServices.CreateScope(Properties, OnConfigureServices).ServiceProvider);

        /// <inheritdoc/>
        IContainer IContainerBuilder.Build() => Build();

        /// <summary>
        /// Build a <typeparamref name="TContainer"/> instance.
        /// </summary>
        /// <param name="ContainerServices"></param>
        /// <returns></returns>
        protected virtual TContainer Build(IServiceProvider ContainerServices) 
            => ContainerServices.GetRequiredService<IServiceInjector>().Create(typeof(TContainer)) as TContainer;

        /// <summary>
        /// Configure the container services to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="Services"></param>
        protected abstract void OnConfigureServices(IServiceCollection Services);
    }
}
