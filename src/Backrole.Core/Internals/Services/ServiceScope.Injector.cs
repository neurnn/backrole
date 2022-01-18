using Backrole.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Backrole.Core.Internals.Services
{
    internal partial class ServiceScope
    {
        /// <summary>
        /// Service Injector.
        /// </summary>
        internal class Injector : IServiceInjector
        {
            private const BindingFlags BF_PUBLICS = BindingFlags.Instance | BindingFlags.Public;
            private const BindingFlags BF_HIDDENS = BindingFlags.Instance | BindingFlags.NonPublic;
            private ServiceScope m_Scope;

            /// <summary>
            /// Initialize a new <see cref="Injector"/> instance.
            /// </summary>
            /// <param name="Scope"></param>
            public Injector(ServiceScope Scope) => m_Scope = Scope;

            /// <inheritdoc/>
            public object Create(Type Target, params object[] Parameters)
            {
                var Instance = Target.CreateWithInjection(m_Scope.GetService, Parameters);
                InjectServicesToMembers(Target, Instance);
                return Instance;
            }

            /// <summary>
            /// Inject all fields and properties.
            /// </summary>
            /// <param name="Target"></param>
            /// <param name="Instance"></param>
            private void InjectServicesToMembers(Type Target, object Instance)
            {
                var Injector = m_Scope.GetRequiredService<IServiceInjector>();
                var Activator
                    =  Target.GetMethod("OnServiceInjected", BF_HIDDENS)
                    ?? Target.GetMethod("OnServiceInjected", BF_PUBLICS);

                var Members = ExtractMembers(Target)
                    .Select(X => (Member: X, Attribute: X.GetCustomAttribute<InjectionAttribute>(true)))
                    .Where(X => X.Attribute != null);

                foreach (var Each in Members)
                    InjectTo(Instance, Each.Member, Each.Attribute);

                /* Call the awake. */
                if (Activator != null)
                    Injector.Invoke(Activator, Instance);
            }

            /// <summary>
            /// Extract the member informations who are property or field.
            /// </summary>
            /// <param name="Target"></param>
            /// <returns></returns>
            private IEnumerable<MemberInfo> ExtractMembers(Type Target) => Target
                .GetFields(BF_PUBLICS).Select(X => X as MemberInfo)
                .Concat(Target.GetFields(BF_HIDDENS))
                .Concat(Target.GetProperties(BF_PUBLICS))
                .Concat(Target.GetProperties(BF_HIDDENS))
                .OrderBy(X => (int)X.MemberType * 10);

            /// <summary>
            /// Inject services to the member.
            /// </summary>
            /// <param name="Target"></param>
            /// <param name="Member"></param>
            /// <param name="Attribute"></param>
            /// <returns></returns>
            private void InjectTo(object Target, MemberInfo Member, InjectionAttribute Attribute)
            {
                if (!ExtractMemberInfo(Target, Member, out var Setter, out var MemberType))
                    return;

                if (Attribute is ServiceInjectionAttribute ServiceInfo)
                {
                    var Service = m_Scope.GetService(MemberType, Member, ServiceInfo);
                    if (Service is null && ServiceInfo.Required)
                        throw new InvalidOperationException($"Couldn't resolve the member, {Member.Name}.");

                    var InstanceType = Service != null ? Service.GetType() : null;
                    if (InstanceType != null && !MemberType.IsAssignableFrom(InstanceType))
                        throw new InvalidOperationException($"The member type is {MemberType.Name}, but the required type is {MemberType.FullName} ({InstanceType.FullName}).");

                    Setter(Service);
                }

                else
                {
                    var Service = m_Scope.GetService(MemberType, Member);
                    var InstanceType = Service != null ? Service.GetType() : null;

                    if (InstanceType != null && !MemberType.IsAssignableFrom(InstanceType))
                        throw new InvalidOperationException($"The member type is {MemberType.Name}, but the required type is {MemberType.FullName} ({InstanceType.FullName}).");

                    Setter(Service);
                }
            }

            /// <summary>
            /// Extracts the necessary member informations and setter delegate from the <see cref="MemberInfo"/>.
            /// </summary>
            /// <param name="Target"></param>
            /// <param name="Member"></param>
            /// <param name="Setter"></param>
            /// <param name="RequiredType"></param>
            /// <returns></returns>
            private static bool ExtractMemberInfo(object Target, MemberInfo Member, out Action<object> Setter, out Type RequiredType)
            {
                if (Member is FieldInfo Field)
                {
                    Setter = Value => Field.SetValue(Target, Value);
                    RequiredType = Field.FieldType;
                    return true;
                }

                else if (Member is PropertyInfo Property)
                {
                    if (!Property.CanWrite)
                        throw new InvalidOperationException($"{Member.Name} isn't writable.");

                    Setter = Value => Property.SetValue(Target, Value);
                    RequiredType = Property.PropertyType;
                    return true;
                }

                Setter = null;
                RequiredType = null;
                return false;
            }

            /// <inheritdoc/>
            public object Invoke(MethodInfo Method, object Target, params object[] Parameters)
                => Method.InvokeWithInjection(Target, m_Scope.GetService, Parameters);
        }
    }
}
