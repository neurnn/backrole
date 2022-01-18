using Backrole.Core.Abstractions;
using System;

namespace Backrole.Core.Builders
{
    /// <summary>
    /// Basic implementation of the <see cref="IContainerBuilderFactory"/> that uses dependency injection to create builder.
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    public class ContainerBuilderFactory<TBuilder> : IContainerBuilderFactory where TBuilder : IContainerBuilder
    {
        /// <inheritdoc/>
        public virtual IContainerBuilder Create(IServiceProvider HostServices)
        {
            var Injector = HostServices.GetRequiredService<IServiceInjector>();
            return Injector.Create(typeof(TBuilder)) as IContainerBuilder;
        }

        /// <inheritdoc/>
        public virtual IContainer Build(IContainerBuilder Builder) => Builder.Build();
    }
}
