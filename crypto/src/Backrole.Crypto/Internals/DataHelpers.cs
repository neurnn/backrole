using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Crypto.Internals
{
    internal static class DataHelpers
    {
        /// <summary>
        /// Make <see cref="BigInteger"/> from the byte array.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static BigInteger ToBigInteger(this byte[] Input, bool ByLoop = false)
        {
            if (ByLoop)
            {
                BigInteger Result = 0;
                for (var i = 0; i < Input.Length; i++)
                    Result = (Result << 8) + Input[i];
                return Result;
            }

            var Sign = new byte[Input.Length + 1];
            Buffer.BlockCopy(Input, 0, Sign, 1, Input.Length);
            Array.Reverse(Sign);
            return new BigInteger(Sign);
        }

        /// <summary>
        /// Stringify the <see cref="BigInteger"/> as N bit number string.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Digits"></param>
        /// <returns></returns>
        public static string AsNumberString(this BigInteger Input, string Digits)
        {
            var Base = Digits.Length;
            var Result = new StringBuilder();

            while (Input > 0)
            {
                var N = (int)(Input % Base); Input /= Base;
                Result.Insert(0, Digits[N]);
            }

            return Result.ToString();
        }

        /// <summary>
        /// Parse the input string to <see cref="BigInteger"/> as N bit number string.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Digits"></param>
        /// <returns></returns>
        public static BigInteger AsBigInteger(this string Input, string Digits)
        {
            var Base = Digits.Length;
            BigInteger Data = 0;

            if (Input.Count(Digits.Contains) != Input.Length)
                throw new FormatException("Input string has invalid digit character.");

            for (int i = 0; i < Input.Length; i++)
                Data = Data * Base + Digits.IndexOf(Input[i]);

            return Data;
        }

        /// <summary>
        /// Counts the leading elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Input"></param>
        /// <param name="Leading"></param>
        /// <returns></returns>
        public static int Leadings<T>(this IEnumerable<T> Input, T Leading) where T : IEquatable<T>
        {
            return Input.TakeWhile(X => X.Equals(Leading)).Count();
        }

        /// <summary>
        /// Convert the <see cref="BigInteger"/> to the byte array.
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="StripSigns"></param>
        /// <param name="AsBigEndian"></param>
        /// <returns></returns>
        public static byte[] MakeByteArray(this BigInteger Number, bool StripSigns = false, bool AsBigEndian = true)
        {
            var Bytes = Number.ToByteArray() as IEnumerable<byte>;

            if (AsBigEndian)
                Bytes = Bytes.Reverse();

            if (StripSigns)
                Bytes = Bytes.SkipWhile(X => X == 0);

            return Bytes.ToArray();
        }

        /// <summary>
        /// Repeat an element as much as specified <paramref name="N"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Input"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        public static IEnumerable<T> Repeat<T>(this T Input, int N) => Enumerable.Repeat(Input, N);
    }
}
