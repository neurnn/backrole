using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Internals.Mappers
{
    internal class ClassMappedEndpoint : IHttpRouterEndpoint
    {
        private Type m_TargetType;
        private MethodInfo m_Method;

        private Func<IHttpContext, Func<Task>, Task> m_Filter;

        /// <summary>
        /// Initialize a new <see cref="ClassMappedEndpoint"/> instance.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="Method"></param>
        public ClassMappedEndpoint(Type TargetType, MethodInfo Method)
        {
            m_TargetType = TargetType;
            m_Method = Method;

            PrepareFilter(TargetType, Method);
        }

        /// <summary>
        /// Prepare the filter asynchronously.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="Method"></param>
        private void PrepareFilter(Type TargetType, MethodInfo Method)
        {
            var Filters = TargetType
                .GetCustomAttributes<HttpExecutionFilterAttribute>(true)
                .Concat(Method.GetCustomAttributes<HttpExecutionFilterAttribute>(true))
                .OrderBy(X => X.Priority);

            foreach (var Each in Filters)
            {
                if (m_Filter != null)
                    m_Filter = new Combine(m_Filter, Each.InvokeAsync).InvokeAsync;

                else
                    m_Filter = Each.InvokeAsync;
            }
        }

        /// <summary>
        /// Combine two middlewares to single delegate.
        /// </summary>
        private struct Combine
        {
            private Func<IHttpContext, Func<Task>, Task> m_Prev;
            private Func<IHttpContext, Func<Task>, Task> m_Next;

            public Combine(Func<IHttpContext, Func<Task>, Task> Prev, Func<IHttpContext, Func<Task>, Task> Next)
            {
                m_Prev = Prev;
                m_Next = Next;
            }

            private struct MakeNext
            {
                private Func<IHttpContext, Func<Task>, Task> m_Next;
                private Func<Task> m_FinalNext;
                private IHttpContext m_Context;

                public MakeNext(Func<IHttpContext, Func<Task>, Task> Next, Func<Task> FinalNext, IHttpContext Context)
                {
                    m_Next = Next;
                    m_FinalNext = FinalNext;
                    m_Context = Context;
                }

                [DebuggerHidden]
                public Task InvokeAsync() => m_Next(m_Context, m_FinalNext);
            }

            [DebuggerHidden]
            public Task InvokeAsync(IHttpContext Context, Func<Task> Next)
                => m_Prev(Context, new MakeNext(m_Next, Next, Context).InvokeAsync);
        }

        /// <summary>
        /// Get an instance of the <paramref name="TargetType"/>.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="Http"></param>
        /// <returns></returns>
        private static object GetInstance(Type TargetType, IHttpContext Http)
        {
            if (Http.Properties.TryGetValue(TargetType, out var Instance))
                return Instance;

            var Injector = Http.Services.GetRequiredService<IServiceInjector>();
            return Http.Properties[TargetType] = Injector.Create(TargetType);
        }

        /// <summary>
        /// Remove an instance of the <paramref name="TargetType"/>.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="Http"></param>
        private static void UnsetInstance(Type TargetType, IHttpContext Http)
            => Http.Properties.Remove(TargetType);

        /// <summary>
        /// Make a factory delegate that invokes <see cref="InvokeAsync(IHttpContext)"/> finally.
        /// </summary>
        /// <returns></returns>
        public Func<IHttpContext, Task<IHttpRouterEndpoint>> MakeFactory() => AsFactory;

        /// <summary>
        /// As Factory.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private Task<IHttpRouterEndpoint> AsFactory(IHttpContext _)
            => Task.FromResult<IHttpRouterEndpoint>(this);

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Http)
        {
            if (m_Filter != null)
                return m_Filter(Http, () => InternalInvokeAsync(Http));

            return InternalInvokeAsync(Http);
        }

        /// <summary>
        /// Invoke the endpoint asynchronously.
        /// </summary>
        /// <param name="Http"></param>
        /// <returns></returns>
        private async Task InternalInvokeAsync(IHttpContext Http)
        {
            var Injector = Http.Services.GetRequiredService<IServiceInjector>();
            var Instance = m_Method.IsStatic ? null : GetInstance(m_TargetType, Http);

            Http.Response.Status = 200;
            Http.Response.StatusPhrase = null;

            try
            {
                var RetVal = Injector.Invoke(m_Method, Instance);
                if (RetVal is Task<IHttpResult> TaskResult)
                    RetVal = await TaskResult;

                if (RetVal is Task Task)
                    await Task;

                if (RetVal is IHttpResult Result)
                    await Result.InvokeAsync(Http);
            }

            finally
            {
                UnsetInstance(m_TargetType, Http);

                if (Instance is IAsyncDisposable Async)
                    await Async.DisposeAsync();

                else if (Instance is IDisposable Sync)
                    Sync.Dispose();
            }
        }
    }
}
