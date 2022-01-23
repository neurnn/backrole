using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Backrole.Http.Internals.Endpoints
{
    /// <summary>
    /// Combine two middlewares to single delegate.
    /// </summary>
    internal struct HttpDelegateCombine<ContextType>
    {
        private Func<ContextType, Func<Task>, Task> m_Prev;
        private Func<ContextType, Func<Task>, Task> m_Next;

        public HttpDelegateCombine(Func<ContextType, Func<Task>, Task> Prev, Func<ContextType, Func<Task>, Task> Next)
        {
            m_Prev = Prev;
            m_Next = Next;
        }

        private struct MakeNext
        {
            private Func<ContextType, Func<Task>, Task> m_Next;
            private Func<Task> m_FinalNext;
            private ContextType m_Context;

            public MakeNext(Func<ContextType, Func<Task>, Task> Next, Func<Task> FinalNext, ContextType Context)
            {
                m_Next = Next;
                m_FinalNext = FinalNext;
                m_Context = Context;
            }

            [DebuggerHidden]
            public Task InvokeAsync() => m_Next(m_Context, m_FinalNext);
        }

        [DebuggerHidden]
        public Task InvokeAsync(ContextType Context, Func<Task> Next)
            => m_Prev(Context, new MakeNext(m_Next, Next, Context).InvokeAsync);
    }
}
