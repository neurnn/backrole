namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Defines the lifetime of the service instance.
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// Only one instance can exist across a service provider.
        /// </summary>
        Singleton,

        /// <summary>
        /// Only one instance can exist per the service scope.
        /// </summary>
        Scoped,

        /// <summary>
        /// Created when every calls.
        /// </summary>
        Transient,

        /// <summary>
        /// One for each node in the scope tree.
        /// </summary>
        Hierarchial
    }
}
