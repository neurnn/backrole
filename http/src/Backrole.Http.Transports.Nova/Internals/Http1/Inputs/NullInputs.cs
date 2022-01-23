using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1.Inputs
{
    internal class NullInputs : BaseInputs
    {
        private static readonly Task<int> ALWAYS_ZERO = Task.FromResult(0);

        /// <inheritdoc/>
        public override Task<int> ReadAsync(ArraySegment<byte> Buffer, CancellationToken Cancellation) => ALWAYS_ZERO;

        /// <inheritdoc/>
        public override ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
