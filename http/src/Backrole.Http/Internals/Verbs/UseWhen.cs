using Backrole.Core.Abstractions;
using Backrole.Http.Abstractions;
using Backrole.Http.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Internals.Verbs
{
    using MiddlewareDelegate = Func<IHttpContext, Func<Task>, Task>;
    using AsyncConditionDelegate = Func<IHttpContext, Task<bool>>;
    using ConditionDelegate = Func<IHttpContext, bool>;

    internal class UseWhen
    {
        private AsyncConditionDelegate m_Condition;

        /// <summary>
        /// Initialize a new <see cref="UseWhen"/> instance.
        /// </summary>
        /// <param name="HttpServices"></param>
        /// <param name="Properties"></param>
        public UseWhen(IHttpApplicationBuilder AppBuilder)
            => Builder = new HttpMiddlewareBuilder(AppBuilder.HttpServices, AppBuilder.Properties);

        /// <summary>
        /// <see cref="IHttpMiddlewareBuilder"/> instance.
        /// </summary>
        public IHttpMiddlewareBuilder Builder { get; }

        /// <summary>
        /// Add condition.
        /// </summary>
        /// <param name="Condition"></param>
        /// <returns></returns>
        public UseWhen When(AsyncConditionDelegate Condition)
        {
            if (m_Condition is null)
                m_Condition = Condition;

            else
            {
                var Previous = m_Condition;
                m_Condition = async (Http) =>
                {
                    return
                        await Previous(Http) &&
                        await Condition(Http);
                };
            }

            return this;
        }

        /// <summary>
        /// Add condition.
        /// </summary>
        /// <param name="Condition"></param>
        /// <returns></returns>
        public UseWhen When(ConditionDelegate Condition)
            => When(Http => Task.FromResult(Condition(Http)));

        /// <summary>
        /// Builds the middleware that is invoked only when the condition met.
        /// </summary>
        /// <returns></returns>
        public MiddlewareDelegate Build()
        {
            if (m_Condition is null) return Builder.Build();
            return new Conditional(m_Condition, Builder.Build()).InvokeAsync;
        }
    }
}
