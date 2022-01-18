using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Services
{
    internal class ServiceDisposables : IAsyncDisposable
    {
        private Stack<object> m_Objects = new();
        private Action m_PostDispose;

        /// <summary>
        /// Set the delegate that should be invoked after all instances are disposed.
        /// </summary>
        /// <param name="Action"></param>
        /// <returns></returns>
        public ServiceDisposables SetPostDispose(Action Action)
        {
            m_PostDispose = Action;
            return this;
        }

        /// <summary>
        /// Reserve to dispose the instance.
        /// </summary>
        /// <param name="Instance"></param>
        /// <returns></returns>
        public bool Reserve(object Instance)
        {
            lock(this)
            {
                if (!(Instance is IDisposable) &&
                    !(Instance is IAsyncDisposable))
                    return false;

                if (m_Objects.Contains(Instance))
                    return false;

                m_Objects.Push(Instance);
                return true;
            }
        }

        /// <summary>
        /// Dispose all reserved instances.
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            Action PostDispose;
            while(true)
            {
                object Instance;
                lock(this)
                {
                    if (!m_Objects.TryPop(out Instance))
                    {
                        PostDispose = m_PostDispose;
                        m_PostDispose = null;
                        break;
                    }
                }

                if (Instance is IAsyncDisposable Async)
                    await Async.DisposeAsync();

                else if (Instance is IDisposable Sync)
                    Sync.Dispose();
            }

            PostDispose?.Invoke();
        }
    }
}
