using Backrole.Core.Abstractions;
using Backrole.Core.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backrole.Core
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds all environment variables to the <see cref="IConfigurationBuilder"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddEnvironmentVariables(this IConfigurationBuilder This, Action<EnvironmentVariableOptions> Configure = null)
        {
            var Options = new EnvironmentVariableOptions();
            Configure?.Invoke(Options);

            var Variables = Environment.GetEnvironmentVariables();
            var KeyValues = new List<KeyValuePair<string, string>>();

            foreach(var Name in Variables.Keys)
            {
                var Value = Variables[Name];
                var NameString = Name != null ? Name.ToString() : null;
                var ValueString = Value != null ? Value.ToString() : null;

                if (NameString is null || ValueString is null)
                    continue;

                if (Options.AsLowerCase)
                    NameString = NameString.ToLower();

                if (Options.Filters.Count > 0 && Options.Filters
                    .Select(X => X(NameString, ValueString))
                    .Count(X => X) <= 0)
                    continue;

                This.Set($"{Options.Prefix}{NameString}", ValueString);
            }

            return This;
        }
    }
}
