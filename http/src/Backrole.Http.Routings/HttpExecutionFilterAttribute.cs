using Backrole.Http.Abstractions;
using System;
using System.Threading.Tasks;

namespace Backrole.Http.Routings
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class HttpExecutionFilterAttribute : Attribute
    {
        /// <summary>
        /// Priority of the filter if required. (default: 0).
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Called to invoke the execution delegate.
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="Execution"></param>
        /// <returns></returns>
        public virtual Task InvokeAsync(IHttpContext Context, Func<Task> Execution) => Execution();
    }
}
