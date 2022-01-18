using System;

namespace Backrole.Core
{
    /// <summary>
    /// Markup the property that it should be filled by the dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ServiceInjectionAttribute : InjectionAttribute
    {
        /// <summary>
        /// Indicates whether the injection should be failure if no service found.
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Service Type to resolve. the property (or field) should be assigned from the specified type.
        /// </summary>
        public Type ServiceType { get; set; }
    }
}
