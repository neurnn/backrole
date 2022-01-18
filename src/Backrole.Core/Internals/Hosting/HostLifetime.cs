using Backrole.Core.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Core.Internals.Hosting
{
    internal class HostLifetime : IHostLifetime, IAsyncDisposable, IDisposable
    {
        private Task m_Disposing;
        private CancellationTokenSource
            m_Started = new(),
            m_Stopping = new(),
            m_Stopped = new();

        [ServiceInjection]
        private ILogger<IHostLifetime> m_Logger = null;

        /// <inheritdoc/>
        public CancellationToken Started => m_Started.Token;

        /// <inheritdoc/>
        public CancellationToken Stopping => m_Stopping.Token;

        /// <inheritdoc/>
        public CancellationToken Stopped => m_Stopped.Token;

        /// <summary>
        /// Trigger the <see cref="CancellationToken"/> and returns it is already triggered or not.
        /// </summary>
        /// <param name="Cts"></param>
        /// <returns></returns>
        private bool Trigger(CancellationTokenSource Cts)
        {
            lock(Cts)
            {
                if (Cts.IsCancellationRequested)
                    return false;

                Cts.Cancel();
                return true;
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (Trigger(m_Stopping))
                m_Logger.Trace("Stopping requested.");
        }

        /// <summary>
        /// Notify the lifetime has been started.
        /// </summary>
        public bool OnStarted()
        {
            if (Trigger(m_Started))
            {
                m_Logger.Trace($"{nameof(Started)} signal propagated.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Notify the lifetime has been stopped.
        /// </summary>
        public bool OnStopped()
        {
            if (Trigger(m_Stopped))
            {
                m_Logger.Trace($"{nameof(Stopped)} signal propagated.");
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            lock(this)
            {
                if (m_Disposing is null)
                    m_Disposing = OnDisposeAsync();

                return new ValueTask(m_Disposing);
            }
        }

        /// <summary>
        /// Trigger all events and dispose them all.
        /// </summary>
        /// <returns></returns>
        private Task OnDisposeAsync() => Task.Run(() =>
        {
            Trigger(m_Stopping);
            m_Started.Dispose();
            m_Stopping.Dispose();
            m_Stopped.Dispose();
        });
    }
}
