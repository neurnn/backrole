using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Backrole.Core.Internals
{
    internal static class TypeExtensions
    {
        private static T InternalMakeDefault<T>() => default;
        private static readonly object[] EMPTY_ARGS = new object[0];
        private static readonly MethodInfo MTD_MAKE_DEFAULT = typeof(TypeExtensions)
            .GetMethod(nameof(InternalMakeDefault), BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Make Type Array from the instance array.
        /// </summary>
        /// <param name="Instances"></param>
        /// <returns></returns>
        private static Type[] MakeTypeArray(object[] Instances)
            => Instances.Select(X => X is null ? null : X.GetType()).ToArray();

        /// <summary>
        /// Make a default value of the <see cref="Type"/>.
        /// This is useful for value types. other types will always return null.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static object MakeDefault(this Type Type)
            => MTD_MAKE_DEFAULT.MakeGenericMethod(Type).Invoke(null, EMPTY_ARGS);

        /// <summary>
        /// Match the given <paramref name="Types"/> array with parameter informations.
        /// </summary>
        /// <param name="Types"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        private static bool MatchParameters(Type[] Types, ParameterInfo[] Params, int Offset = 0)
        {
            if (Types.Length != Params.Length - Offset)
                return false;

            for (int i = 0, j = Offset; i < Types.Length; ++i, ++j)
            {
                if (Types[i] is null)
                    continue;

                var Param = Params[j].ParameterType;
                if (Param.IsAssignableFrom(Types[i]))
                    continue;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Make Null Parameters to be default value.
        /// </summary>
        /// <param name="Parameters"></param>
        /// <param name="Params"></param>
        private static void MakeNullToDefaultParameter(object[] Parameters, ParameterInfo[] Params, int Offset = 0)
        {
            for (int i = 0, j = Offset; i < Parameters.Length; ++i, ++j)
            {
                if (Parameters[i] != null)
                    continue;

                if (Params[j].HasDefaultValue)
                {
                    Parameters[i] = Params[j].DefaultValue;
                    continue;
                }

                Parameters[i] = MakeDefault(Params[j].ParameterType);
            }
        }

        /// <summary>
        /// Create an instance of the <paramref name="Type"/> with dependency injection.
        /// This invokes <paramref name="Callback"/> to resolve a parameters.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Callback"></param>
        /// <param name="Parameters"></param>
        /// <exception cref="TargetInvocationException">Failed to invoke the constructor.</exception>
        /// <exception cref="InvalidOperationException">(nested) Tried to instantiate abstract or interface type.</exception>
        /// <exception cref="NotSupportedException">(nested) No suitable constructor choosen or failed to resolve parameters.</exception>
        /// <returns></returns>
        public static object CreateWithInjection(this Type Type, Func<ParameterInfo, object> Callback, params object[] Parameters)
        {
            if (Type.IsAbstract || Type.IsInterface)
            {
                throw new TargetInvocationException(new InvalidOperationException(
                    "No abstract or interface type can be instantiated."));
            }
            
            var Ctors = Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Concat(Type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance))
                .Select(X => (Ctor: X, Params: X.GetParameters()))
                .OrderByDescending(X => X.Params.Length);

            var ParamTypes = MakeTypeArray(Parameters ?? EMPTY_ARGS);

            var CtorBest = null as ConstructorInfo;
            var ParamBest = null as ParameterInfo[];
            int OffsetBest = 0;

            foreach (var Ctor in Ctors)
            {
                var Params = Ctor.Params;
                if (Params.Length < Parameters.Length)
                    continue;

                var Offset = Params.Length - Parameters.Length;
                if (MatchParameters(ParamTypes, Params, Offset))
                {
                    CtorBest = Ctor.Ctor;
                    ParamBest = Params;
                    OffsetBest = Offset;
                    break;
                }
            }

            if (CtorBest is null)
            {
                throw new TargetInvocationException(new NotSupportedException(
                    "No suitable constructor choosen."));
            }

            MakeNullToDefaultParameter(Parameters, ParamBest, OffsetBest);
            Array.Resize(ref Parameters, ParamBest.Length);
            Array.Copy(Parameters, 0, Parameters, OffsetBest, Parameters.Length - OffsetBest);

            for (var i = 0; i < OffsetBest; ++i)
            {
                var Current = ParamBest[i];

                if ((Parameters[i] = Callback(Current)) is null &&
                    (Parameters[i] = Current.ParameterType.MakeDefault()) is null)
                {
                    throw new TargetInvocationException(new NotSupportedException(
                        $"No parameter resolved for {Current.Name} ({Current.ParameterType.FullName}) of {Type.FullName}."));
                }
            }

            try { return CtorBest.Invoke(Parameters); }
            catch (TargetInvocationException Exception)
            {
                ExceptionDispatchInfo.Capture(Exception).Throw();
                throw;
            }
        }

        /// <summary>
        /// Invoke the method of the <paramref name="Target"/> with dependency injection.
        /// This invokes <paramref name="Callback"/> to resolve a parameters.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Callback"></param>
        /// <param name="Parameters"></param>
        /// <exception cref="TargetInvocationException">Failed to invoke the constructor.</exception>
        /// <exception cref="InvalidOperationException"> (nested) The <paramref name="Method"/> is static but target object is specified.</exception>
        /// <exception cref="NotSupportedException"> (nested) No suitable constructor choosen or failed to resolve parameters.</exception>
        /// <returns></returns>
        public static object InvokeWithInjection(this MethodInfo Method, object Target, Func<ParameterInfo, object> Callback, params object[] Parameters)
        {
            if (Method.IsStatic && Target != null)
            {
                throw new TargetInvocationException(new InvalidOperationException(
                    "The method is static but target object is specified."));
            }

            var Params = Method.GetParameters();
            var Offset = Params.Length - Parameters.Length;

            MakeNullToDefaultParameter(Parameters, Params, Offset);
            Array.Resize(ref Parameters, Params.Length);
            Array.Copy(Parameters, 0, Parameters, Offset, Parameters.Length - Offset);

            for (var i = 0; i < Offset; ++i)
            {
                var Current = Params[i];

                if ((Parameters[i] = Callback(Current)) is null &&
                    (Parameters[i] = Current.ParameterType.MakeDefault()) is null)
                {
                    throw new TargetInvocationException(new NotSupportedException(
                        $"No parameter resolved for {Current.Name} ({Current.ParameterType.FullName}) of {Method.Name}."));
                }
            }

            try { return Method.Invoke(Target, Parameters); }
            catch (TargetInvocationException Exception)
            {
                ExceptionDispatchInfo.Capture(Exception).Throw();
                throw;
            }
        }

    }
}
