using Backrole.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Hosting
{
    internal class HostContainerLocator : IContainerLocator
    {
        private TaskCompletionSource m_Ready = new();

        [ServiceInjection]
        private ILogger<IContainerLocator> m_Logger = null;

        /// <summary>
        /// Containers.
        /// </summary>
        public List<IContainer> Containers { get; } = new List<IContainer>();

        /// <inheritdoc/>
        public Task Ready => m_Ready.Task;

        /// <inheritdoc/>
        public IEnumerable<IContainer> All() => Containers;

        /// <inheritdoc/>
        public IContainer Find(Predicate<IContainer> Condition)
            => Containers.Find(Condition);

        /// <inheritdoc/>
        public IContainer FindLast(Predicate<IContainer> Condition)
            => Containers.FindLast(Condition);

        /// <inheritdoc/>
        public IEnumerable<IContainer> FindAll(Predicate<IContainer> Condition)
            => Containers.FindAll(Condition);

        /// <summary>
        /// Notify the signal that represents the locator is ready.
        /// </summary>
        public void OnReady()
        {
            m_Logger.Debug($"`{nameof(OnReady)}` method get called.");
            m_Ready.TrySetResult();
        }
    }
}
