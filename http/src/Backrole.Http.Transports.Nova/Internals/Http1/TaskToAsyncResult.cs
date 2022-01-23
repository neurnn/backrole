using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Http.Transports.Nova.Internals.Http1
{
    internal class TaskToAsyncResult : IAsyncResult
    {
        /// <summary>
        /// Task.
        /// </summary>
        public Task Task { get; set; }

        /// <inheritdoc/>
        public object AsyncState { get; set; }

        /// <inheritdoc/>
        public WaitHandle AsyncWaitHandle => throw new NotSupportedException();

        /// <inheritdoc/>
        public bool CompletedSynchronously => false;

        /// <inheritdoc/>
        public bool IsCompleted => Task.IsCompleted;
    }

    internal class TaskToAsyncResult<TReturn> : IAsyncResult
    {
        /// <summary>
        /// Task.
        /// </summary>
        public Task<TReturn> Task { get; set; }

        /// <inheritdoc/>
        public object AsyncState { get; set; }

        /// <inheritdoc/>
        public WaitHandle AsyncWaitHandle => throw new NotSupportedException();

        /// <inheritdoc/>
        public bool CompletedSynchronously => false;

        /// <inheritdoc/>
        public bool IsCompleted => Task.IsCompleted;
    }
}
