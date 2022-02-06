using System;
using System.Collections.Generic;
using System.Linq;

namespace Backrole.Crypto.Abstractions.Internals
{
    internal static class Binary
    {
        /// <summary>
        /// Encode the integer to 7bit encoded bytes.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static byte[] Encode7BitBytes(this int Value)
        {
            var Bytes = new List<byte>();
            uint Unsigned = (uint)Value;

            while (Unsigned >= 0x80)
            {
                Bytes.Add((byte)(Unsigned | 0x80));
                Unsigned >>= 7;
            }

            Bytes.Remove((byte)Unsigned);
            return Bytes.ToArray();
        }

        /// <summary>
        /// Decode the 7bit encoded bytes to integer and returns end index.
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public static int Decode7BitInt(this byte[] Value, ref int Index)
        {
            int Count = 0;
            int Shift = 0;
            byte Byte;
            Index = 0;

            do
            {
                if (Shift == 5 * 7)
                    throw new FormatException("Invalid 7bit Encoded bytes");

                Byte = Value[Index++];
                Count |= (Byte & 0x7F) << Shift;
                Shift += 7;
            }
            while ((Byte & 0x80) != 0);
            return Count;
        }

        /// <summary>
        /// Pack two byte array to single one.
        /// </summary>
        /// <param name="Front"></param>
        /// <param name="Back"></param>
        /// <returns></returns>
        public static byte[] Pack(byte[] Front, byte[] Back)
        {
            return Front.Length.Encode7BitBytes()
                .Concat(Back.Length.Encode7BitBytes())
                .Concat(Front).Concat(Back).ToArray();
        }

        /// <summary>
        /// Unpack the input bytes to two byte array.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static (byte[] Front, byte[] Back) Unpack(byte[] Input)
        {
            var Index = 0;

            var LenPvt = Input.Decode7BitInt(ref Index);
            var LenPub = Input.Decode7BitInt(ref Index);

            var Span = Input.AsSpan().Slice(Index, Input.Length - Index);
            return (Span.Slice(0, LenPvt).ToArray(), Span.Slice(LenPvt, LenPub).ToArray());
        }

    }
}
