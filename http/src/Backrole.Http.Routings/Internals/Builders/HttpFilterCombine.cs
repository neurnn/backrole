using Backrole.Http.Routings.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Internals.Builders
{
    internal struct HttpFilterCombine
    {
        private Func<IHttpRouterContext, Func<Task>, Task> m_Prev, m_Next;

        public HttpFilterCombine(
            Func<IHttpRouterContext, Func<Task>, Task> Prev,
            Func<IHttpRouterContext, Func<Task>, Task> Next)
        {
            m_Prev = Prev;
            m_Next = Next;
        }

        private struct MakeNext
        {
            private IHttpRouterContext m_Context;
            private Func<IHttpRouterContext, Func<Task>, Task> m_Next;
            private Func<Task> m_FinalNext;

            public MakeNext(IHttpRouterContext Context,
                Func<IHttpRouterContext, Func<Task>, Task> Next,
                Func<Task> FinalNext)
            {
                m_Context = Context;
                m_Next = Next;
                m_FinalNext = FinalNext;
            }

            [DebuggerHidden]
            public Task InvokeAsync() => m_Next(m_Context, m_FinalNext);
        }

        [DebuggerHidden]
        public Task InvokeAsync(IHttpRouterContext Context, Func<Task> Next)
            => m_Prev(Context, new MakeNext(Context, m_Next, Next).InvokeAsync);
    }
}
