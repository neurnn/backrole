using Backrole.Http.Abstractions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Backrole.Http.Internals.Verbs
{
    using MiddlewareDelegate = Func<IHttpContext, Func<Task>, Task>;
    using AsyncConditionDelegate = Func<IHttpContext, Task<bool>>;

    internal struct Conditional
    {
        private AsyncConditionDelegate m_Condition;
        private MiddlewareDelegate m_Middleware;

        public Conditional(
            AsyncConditionDelegate Condition,
            MiddlewareDelegate Middleware)
        {
            m_Condition = Condition;
            m_Middleware = Middleware;
        }

        [DebuggerHidden]
        public async Task InvokeAsync(IHttpContext Http, Func<Task> Next)
        {
            if (await m_Condition(Http))
            {
                await m_Middleware(Http, Next);
                return;
            }

            await Next();
        }
    }
}
