using System.Collections.Generic;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Abstracts the place to share data between components.
    /// </summary>
    public interface IServiceProperties : IDictionary<object, object>
    {
    }
}
