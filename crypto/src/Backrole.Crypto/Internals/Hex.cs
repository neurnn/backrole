using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Crypto.Internals
{
    internal static class Hex
    {
        public static string ToHex(this byte[] Bytes)
            => BitConverter.ToString(Bytes).Replace("-", "");

        /// <summary>
        /// Test whether the <paramref name="Value"/> is hex code or not.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private static bool IsHexLow(char Value) => Value >= '0' && Value <= '9' || Value >= 'a' && Value <= 'f';

        /// <summary>
        /// Convert the Hex code to Value.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static int HexVal(this char Value)
        {
            if (Value >= '0' && Value <= '9')
                return Value - '0';

            return Value - 'a' + 10;
        }

        /// <summary>
        /// Test whether the given string is hex or not
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static bool IsHexString(this string This) => This.Count(IsHexLow) == This.Length;
    }
}
