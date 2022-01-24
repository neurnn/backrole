using System;

namespace Backrole.Http.Routings
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HttpRouteAttribute : Attribute
    {
        /// <summary>
        /// Path to map.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Method to handle. comma separated.
        /// e.g. "GET, POST". (Ignored if this specified on the class)
        /// </summary>
        public string Method { get; set; } = "GET, POST";
    }
}
