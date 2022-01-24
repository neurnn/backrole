using System.Collections.Generic;

namespace Backrole.Http.Routings.Abstractions
{
    /// <summary>
    /// Represents the <see cref="IHttpRouter"/> stack's operation state.
    /// </summary>
    public interface IHttpRouterState
    {
        /// <summary>
        /// Current scoped router.
        /// </summary>
        IHttpRouter Current { get; }

        /// <summary>
        /// Routers that scoped.
        /// </summary>
        IEnumerable<IHttpRouter> Routers { get; }

        /// <summary>
        /// Path Names that scoped currently.
        /// </summary>
        IEnumerable<string> PathNames { get; }

        /// <summary>
        /// Path Names that is pending to be scoped.
        /// </summary>
        IEnumerable<string> PendingPathNames { get; }

        /// <summary>
        /// Path Parameters.
        /// </summary>
        IDictionary<string, string> PathParameters { get; }
    }
}
