using Backrole.Core.Abstractions;
using Backrole.Core.Loggings;
using Backrole.Core.Loggings.Internals;
using System;
using System.Collections.Generic;

namespace Backrole.Core
{
    public static class LoggerFactoryBuilderExtensions
    {
        /// <summary>
        /// Add the console logger to logger factory builder.
        /// </summary>
        /// <param name="Builder"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static ILoggerFactoryBuilder AddConsole(this ILoggerFactoryBuilder Builder, Action<ConsoleLoggerOptions> Configure = null)
        {
            var Delegates = Builder.Services.Properties
                .GetValue<List<Action<ConsoleLoggerOptions>>>(typeof(ConsoleLoggerOptions));

            if (Delegates is null)
            {
                Builder.Services.Properties[typeof(ConsoleLoggerOptions)]
                    = Delegates = new List<Action<ConsoleLoggerOptions>>();

                Builder.Add(Services =>
                {
                    var Options = Services.GetRequiredService<IOptions<ConsoleLoggerOptions>>();

                    foreach (var Each in Delegates)
                        Each?.Invoke(Options.Value);

                    return new ConsoleLoggerFactory(Options.Value);
                });
            }

            if (Configure != null)
                Delegates.Add(Configure);

            return Builder;
        }
    }
}
