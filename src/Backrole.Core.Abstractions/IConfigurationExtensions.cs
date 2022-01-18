using System;

namespace Backrole.Core.Abstractions
{
    public static class IConfigurationExtensions
    {
        private delegate bool TryParseDelegate<TValue>(string Input, out TValue Value);

        /// <summary>
        /// Get Value with the <paramref name="Parser"/>.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Parser"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        private static bool TryGet<TValue>(this IConfiguration This, string Key, TryParseDelegate<TValue> Parser, out TValue Value)
        {
            var Temp = This.Get(Key);
            if (Temp is null)
            {
                Value = default;
                return false;
            }

            return Parser(Temp, out Value);
        }

        /// <summary>
        /// Get a value as 16 bit integer.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static short GetInt16(this IConfiguration This, string Key, short Default = 0)
        {
            if (This.TryGet<short>(Key, short.TryParse, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get a value as 32 bit integer.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static int GetInt32(this IConfiguration This, string Key, int Default = 0)
        {
            if (This.TryGet<int>(Key, int.TryParse, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get a value as 64 bit integer.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static long GetInt64(this IConfiguration This, string Key, long Default = 0)
        {
            if (This.TryGet<long>(Key, long.TryParse, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get a value as 16 bit unsigned integer.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static ushort GetUInt16(this IConfiguration This, string Key, ushort Default = 0)
        {
            if (This.TryGet<ushort>(Key, ushort.TryParse, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get a value as 32 bit unsigned integer.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static uint GetUInt32(this IConfiguration This, string Key, uint Default = 0)
        {
            if (This.TryGet<uint>(Key, uint.TryParse, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get a value as 64 bit unsigned integer.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static ulong GetUInt64(this IConfiguration This, string Key, ulong Default = 0)
        {
            if (This.TryGet<ulong>(Key, ulong.TryParse, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get a single floating point number.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static float GetSingle(this IConfiguration This, string Key, float Default = 0)
        {
            if (This.TryGet<float>(Key, float.TryParse, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get a double floating point number.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static double GetDouble(this IConfiguration This, string Key, double Default = 0)
        {
            if (This.TryGet<double>(Key, double.TryParse, out var Value))
                return Value;

            return Default;
        }

        /// <summary>
        /// Get a boolean.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static bool GetBoolean(this IConfiguration This, string Key, bool Default = false)
        {
            var Temp = This.Get(Key);
            if (Temp != null)
            {
                if (Temp.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    Temp.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }

            return Default;
        }

        /// <summary>
        /// Get an enum value.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static TEnum GetEnum<TEnum>(this IConfiguration This, string Key, TEnum Default = default) where TEnum : struct, Enum
        {
            var Temp = This.Get(Key);
            if (Temp != null)
            {
                foreach(var Value in Enum.GetValues<TEnum>())
                {
                    var Name = Enum.GetName(Value);
                    if (Name.Equals(Temp, StringComparison.OrdinalIgnoreCase))
                        return Value;
                }
            }

            return Default;
        }

        /// <summary>
        /// Get a datetime value.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Fallback"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IConfiguration This, string Key, Func<DateTime> Fallback = null)
        {
            var Temp = This.Get(Key);
            if (Temp != null && DateTime.TryParse(Temp, out var Value))
                return Value;

            if (Fallback != null)
                return Fallback();

            return DateTime.MinValue;
        }

        /// <summary>
        /// Get a timespan value.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Fallback"></param>
        /// <returns></returns>
        public static TimeSpan GetTimeSpan(this IConfiguration This, string Key, TimeSpan Default = default)
        {
            var Temp = This.Get(Key);
            if (Temp != null && TimeSpan.TryParse(Temp, out var Value))
                return Value;

            return Default;
        }
    }
}
