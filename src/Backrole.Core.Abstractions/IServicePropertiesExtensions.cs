using System;

namespace Backrole.Core.Abstractions
{
    public static class IServicePropertiesExtensions
    {
        /// <summary>
        /// Try to get a value from the <see cref="IServiceProperties"/> by its key.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="OutValue"></param>
        /// <returns></returns>
        public static bool TryGetValue<TValue>(this IServiceProperties This, object Key, out TValue OutValue)
        {
            if (This.TryGetValue(Key, out var Temp) && Temp is TValue Value)
            {
                OutValue = Value;
                return true;
            }

            OutValue = default;
            return false;
        }

        /// <summary>
        /// Get a value by its key with its fallback delegate that creates a new instance of the <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Fallback"></param>
        /// <returns></returns>
        public static TValue GetValue<TValue>(this IServiceProperties This, object Key, Func<TValue> Fallback = null)
        {
            if (This.TryGetValue<TValue>(Key, out var Value))
                return Value;

            if (Fallback != null && Fallback() is TValue FbValue)
            {
                This[Key] = FbValue;
                return FbValue;
            }

            return default;
        }
    }
}
