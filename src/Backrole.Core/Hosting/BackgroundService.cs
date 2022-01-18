using Backrole.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Core.Hosting
{
    public abstract class BackgroundService : IHostedService
    {
        private TaskCompletionSource m_Tcs;
        private Task m_Task;

        private Action m_RequestStop = null;

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken Token = default)
        {
            lock(this)
            {
                if (m_Task != null)
                    return m_Tcs.Task;

                if (m_Tcs is null)
                    m_Tcs = new TaskCompletionSource();

                m_Task = RunAsync();
                return m_Tcs.Task;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            Task TaskExec, TaskPreparation;
            Action RequestStop;

            lock (this)
            {
                TaskExec = m_Task ?? Task.CompletedTask;
                TaskPreparation = m_Tcs != null ? m_Tcs.Task : Task.CompletedTask;

                RequestStop = m_RequestStop;
                m_RequestStop = null;
            }

            await TaskPreparation;

            try { RequestStop?.Invoke(); }
            catch { }

            await TaskExec;
        }

        /// <summary>
        /// Run the background service and create the stopping method.
        /// </summary>
        /// <returns></returns>
        private async Task RunAsync()
        {
            using (var Cts = new CancellationTokenSource())
            {
                lock(this)
                {
                    m_RequestStop = Cts.Cancel;
                    m_Tcs.TrySetResult();
                }

                await RunAsync(Cts.Token);
            }
        }

        /// <summary>
        /// Run the background service asynchronously.
        /// </summary>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        protected abstract Task RunAsync(CancellationToken Cancellation);
    }
}
