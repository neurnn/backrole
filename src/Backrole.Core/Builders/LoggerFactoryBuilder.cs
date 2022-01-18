using Backrole.Core.Abstractions;
using Backrole.Core.Internals.Fallbacks;
using Backrole.Core.Internals.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backrole.Core.Builders
{
    using FactoryDelegate = Func<IServiceProvider, ILoggerFactory>;

    /// <summary>
    /// Configures the logger factory for the container.
    /// </summary>
    public class LoggerFactoryBuilder : ILoggerFactoryBuilder
    {
        private static readonly object KEY = typeof(LoggerFactoryBuilder);
        private List<FactoryDelegate> m_Delegates;

        /// <summary>
        /// Initialize a new <see cref="LoggerFactoryBuilder"/> instance for the <see cref="IServiceCollection"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        public LoggerFactoryBuilder(IServiceCollection Services) 
            => m_Delegates = (this.Services = Services).Properties.GetValue(KEY, () => new List<FactoryDelegate>());

        /// <inheritdoc/>
        public IServiceCollection Services { get; }

        /// <inheritdoc/>
        public ILoggerFactoryBuilder Clear()
        {
            m_Delegates.Clear();
            return this;
        }

        /// <inheritdoc/>
        public ILoggerFactoryBuilder Add(FactoryDelegate Delegate)
        {
            m_Delegates.Add(Delegate);
            return this;
        }

        /// <inheritdoc/>
        public ILoggerFactory Build(IServiceProvider Services)
        {
            var Injector = Services.GetRequiredService<IServiceInjector>();
            var Factories = m_Delegates.Select(X => X(Services)).ToArray();

            if (Factories.Length <= 0)
                return new NullLoggerFactory();

            if (Factories.Length <= 1)
                return Factories.First();

            return Injector.Create(typeof(LoggerFactory), Factories) as ILoggerFactory;
        }
    }
}
