using System;
using System.Reflection;

namespace Backrole.Core.Abstractions
{
    /// <summary>
    /// Creates an instance of the type or invokes a method of the target instance
    /// with the dependency injection.
    /// </summary>
    public interface IServiceInjector
    {
        /// <summary>
        /// Instantiate an instance with appending parameters.
        /// </summary>
        /// <param name="InstanceType"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        object Create(Type InstanceType, params object[] Parameters);

        /// <summary>
        /// Invoke a method with appending parameters.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Target"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        object Invoke(MethodInfo Method, object Target, params object[] Parameters);
    }
}
