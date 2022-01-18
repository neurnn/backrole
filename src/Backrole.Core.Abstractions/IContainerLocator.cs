using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// An interfact that is to locate an <see cref="IContainer"/> instance.
    /// </summary>
    public interface IContainerLocator
    {
        /// <summary>
        /// Task that completed when the locator is ready.
        /// </summary>
        Task Ready { get; }

        /// <summary>
        /// Get all containers.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IContainer> All();

        /// <summary>
        /// Find an <see cref="IContainer"/> instance that met the condition.
        /// </summary>
        /// <param name="Condition"></param>
        /// <returns></returns>
        IContainer Find(Predicate<IContainer> Condition);

        /// <summary>
        /// Find an last <see cref="IContainer"/> instance that met the condition.
        /// </summary>
        /// <param name="Condition"></param>
        /// <returns></returns>
        IContainer FindLast(Predicate<IContainer> Condition);

        /// <summary>
        /// Find all <see cref="IContainer"/> instances that met the condition.
        /// </summary>
        /// <param name="Condition"></param>
        /// <returns></returns>
        IEnumerable<IContainer> FindAll(Predicate<IContainer> Condition);
    }

}
