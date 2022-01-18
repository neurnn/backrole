using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Core.Abstractions
{
    public static class IHostExtensions
    {
        /// <summary>
        /// Run the <see cref="IHost"/> asynchronously.
        /// </summary>
        /// <param name="Host"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static async Task RunAsync(this IHost Host, CancellationToken Token = default)
        {
            using var Cts = new CancellationTokenSource();
            using var _ = Token.Register(Cts.Cancel);

            /* Receiver for getting the SIGINT signal. */
            void ReceiveInterruptRequest(object _, ConsoleCancelEventArgs Event)
            {
                Event.Cancel = true;
                lock (Cts)
                {
                    if (Cts.IsCancellationRequested)
                        return;

                    Cts.Cancel();
                }
            }

            /* Capture the [Ctrl + C] key combination. */
            Console.CancelKeyPress += ReceiveInterruptRequest;
            try
            {
                var Lifetime = Host.Services.GetRequiredService<IHostLifetime>();
                Cts.Token.ThrowIfCancellationRequested();

                /* Then, proceed to starting the host. */
                try
                {
                    await Host.StartAsync(Cts.Token);
                    using (Cts.Token.Register(Lifetime.Stop))
                    {
                        var Tcs = new TaskCompletionSource();
                        using (Lifetime.Stopping.Register(Tcs.SetResult))
                            await Tcs.Task;
                    }
                }

                finally
                {
                    await Host.StopAsync();
                }
            }

            /* Finally, unregister the capturer. */
            finally { Console.CancelKeyPress -= ReceiveInterruptRequest; }
        }

    }
}
