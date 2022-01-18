using Backrole.Core.Abstractions;
using Backrole.Core.Builders;
using System;

namespace Backrole.Core
{
    public static class ContainerBuilderFactoryExtensions
    {
        /// <summary>
        /// Adds a container that will be built by the <typeparamref name="TContainerBuilder"/>.
        /// Note that this uses <see cref="ContainerBuilderFactory{TBuilder}"/> to create <typeparamref name="TContainerBuilder"/> instance.
        /// </summary>
        /// <typeparam name="TContainerBuilder"></typeparam>
        /// <param name="This"></param>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public static IHostBuilder Configure<TContainerBuilder>(this IHostBuilder This, Action<TContainerBuilder> Builder = null) where TContainerBuilder : class, IContainerBuilder
            => This.Configure(new ContainerBuilderFactory<TContainerBuilder>(), X => Builder?.Invoke(X as TContainerBuilder));
    }
}
