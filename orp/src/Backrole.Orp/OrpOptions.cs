using Backrole.Orp.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Orp
{
    /// <summary>
    /// Orp Options interface.
    /// </summary>
    public class OrpOptions : IOrpOptions
    {
        private Dictionary<string, Type> m_Name2Type = new();
        private Dictionary<Type, string> m_Type2Name = new();

        /// <inheritdoc/>
        public DateTime Epoch { get; set; } = new DateTime(2022, 2, 5, 3, 54, 0, DateTimeKind.Utc);

        /// <inheritdoc/>
        public bool UseLittleEndian { get; set; } = true;

        /// <summary>
        /// Size of the incoming message queue.
        /// </summary>
        public int IncomingQueueSize { get; set; } = 2048;

        /// <inheritdoc/>
        public bool TryGetName(Type Type, out string Name)
        {
            return m_Type2Name.TryGetValue(Type, out Name);
        }

        /// <inheritdoc/>
        public bool TryGetType(string Name, out Type Type)
        {
            return m_Name2Type.TryGetValue(Name, out Type);
        }

        /// <inheritdoc/>
        public IOrpOptions With(Action<IOrpOptions> Delegate)
        {
            Delegate?.Invoke(this);
            return this;
        }

        /// <inheritdoc/>
        public IOrpOptions Map(Type Type, bool Override = false)
        {
            var Attribute = Type.GetCustomAttribute<OrpMessageAttribute>();
            var Name = (Attribute != null ? Attribute.Name : Type.FullName) ?? Type.FullName;

            if (string.IsNullOrWhiteSpace(Name))
                Name = Type.FullName;

            if (m_Name2Type.TryGetValue(Name, out var OtherType) && !Override)
                throw new InvalidOperationException($"the name, {Name} has been used by other type: {OtherType.FullName}.");

            if (m_Type2Name.TryGetValue(Type, out var OtherName) && !Override)
                throw new InvalidOperationException($"the type, {Type.FullName} has been mapped as: {OtherName}.");

            if (m_Name2Type.Remove(Name, out OtherType))
                m_Type2Name.Remove(OtherType);

            if (m_Type2Name.Remove(Type, out OtherName))
                m_Name2Type.Remove(OtherName);

            m_Name2Type[Name] = Type;
            m_Type2Name[Type] = Name;
            return this;
        }

        /// <inheritdoc/>
        public IOrpOptions Map(Assembly Assembly, Predicate<Type> Filter = null, bool Override = false)
        {
            foreach(var Each in Assembly.GetTypes())
            {
                if (Filter is null || Filter(Each))
                    Map(Each, Override);
            }

            return this;
        }

        /// <inheritdoc/>
        public IOrpOptions Unmap(Type Type)
        {
            if (m_Type2Name.Remove(Type, out var Name))
                m_Name2Type.Remove(Name);

            return this;
        }
    }
}
