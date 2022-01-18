namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Markup to make the service is resolved at referenced, rather than immediately. 
    /// </summary>
    /// <typeparam name="ServiceType"></typeparam>
    public interface ILazy<ServiceType>
    {
        /// <summary>
        /// Service instance.
        /// </summary>
        ServiceType Instance { get; }
    }
}
