using System;

namespace Backrole.Orp
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class OrpMessageAttribute : Attribute
    {
        /// <summary>
        /// Name of the ORP message.
        /// </summary>
        public string Name { get; set; }
    }
}
