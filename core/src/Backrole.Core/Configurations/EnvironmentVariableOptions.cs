using System;
using System.Collections.Generic;

namespace Backrole.Core.Configurations
{
    public sealed class EnvironmentVariableOptions
    {
        /// <summary>
        /// Prefix of the environment variable configuration key.
        /// </summary>
        public string Prefix { get; set; } = "env:";

        /// <summary>
        /// Treat all environment variable as lower-case.
        /// </summary>
        public bool AsLowerCase { get; set; } = true;

        /// <summary>
        /// Filters that removes unnecessary environment variables.
        /// </summary>
        public List<Func<string, string, bool>> Filters { get; } = new();
    }
}
