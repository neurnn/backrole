namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Options container for the service.
    /// </summary>
    public interface IOptions
    {
        /// <summary>
        /// Option values.
        /// </summary>
        object Value { get; }
    }

    /// <summary>
    /// Options container for the service.
    /// (Generic version of the <see cref="IOptions"/>)
    /// </summary>
    /// <typeparam name="ValueType"></typeparam>
    public interface IOptions<ValueType> : IOptions where ValueType : class
    {
        /// <summary>
        /// Option values.
        /// </summary>
        new ValueType Value { get; }
    }
}
