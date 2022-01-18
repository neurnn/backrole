using Backrole.Core.Abstractions;

namespace Backrole.Core.Internals.Services
{
    internal class Options<ValueType> : IOptions<ValueType> where ValueType : class
    {
        /// <summary>
        /// Initialize a new <see cref="Options{ValueType}"/> instance.
        /// </summary>
        public Options(IServiceInjector Injector)
            => Value = Injector.Create(typeof(ValueType)) as ValueType;

        /// <inheritdoc/>
        public ValueType Value { get; }

        /// <inheritdoc/>
        object IOptions.Value => Value;
    }
}
