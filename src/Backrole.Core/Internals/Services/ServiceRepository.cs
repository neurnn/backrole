using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Services
{
    internal class ServiceRepository
    {
        private Dictionary<Type, Task<object>> m_Tasks = new();

        /// <summary>
        /// Clear the repository.
        /// </summary>
        public void Clear()
        {
            lock (this)
                m_Tasks.Clear();
        }

        /// <summary>
        /// Gets the reserved service instance as task.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public Task<object> GetReserved(Type ServiceType)
        {
            if (m_Tasks.TryGetValue(ServiceType, out var Value))
                return Value;

            return null;
        }

        /// <summary>
        /// Reserve the <paramref name="ServiceType"/> for the <see cref="ServiceRepository"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Tcs"></param>
        /// <returns></returns>
        public Task<object> Reserve(Type ServiceType, out TaskCompletionSource<object> Tcs)
        {
            lock(this)
            {
                if (m_Tasks.TryGetValue(ServiceType, out var Temp))
                {
                    Tcs = null;
                    return Temp;
                }

                Tcs = new TaskCompletionSource<object>();
                m_Tasks.Add(ServiceType, Tcs.Task);
                return Tcs.Task;
            }
        }
    }
}
