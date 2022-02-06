using Backrole.Crypto.Internals;
using System;
using System.Linq;

namespace Backrole.Crypto
{
    public static class Base58
	{
		private const int SIZE_CHKSUM = 4;

		/// <summary>
		/// Uses `Bitcoin` style digits.
		/// </summary>
		public const string Bitcoin = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

		/// <summary>
		/// Uses `Flicker` style digits.
		/// </summary>
		public const string Flicker = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";

		/// <summary>
		/// Uses `Reverse (Bitcoin)` style digits.
		/// </summary>
		public const string Reverse = "zyxwvutsrqponmkjihgfedcbaZYXWVUTSRQPNMLKJHGFEDCBA987654321";

		/// <summary>
		/// Uses `Glazer` (neurnn corp) style digits.
		/// </summary>
		public const string Glazer = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

		/// <summary>
		/// Encode the Input bytes to <see cref="Base58"/> encoded string.
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Checksum"></param>
		/// <returns></returns>
		public static string ToBase58(this byte[] Input, bool Checksum = false, string Digits = Glazer)
		{
			if (Checksum)
			{
				var Hash = Hashes.Default.Hash("DSHA256", Input).Value;
				Array.Resize(ref Input, Input.Length + SIZE_CHKSUM);
				Buffer.BlockCopy(Hash, 0, Input, Input.Length - SIZE_CHKSUM, SIZE_CHKSUM);
			}

			var Result = Input.ToBigInteger().AsNumberString(Digits);
			for (int i = 0; i < Input.Length && Input[i] == 0; i++)
				Result = Digits[0] + Result;

			return Result;
		}

		/// <summary>
		/// Decode the Input string to <see cref="Base58"/> decoded bytes.
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Checksum"></param>
		/// <returns></returns>
		public static byte[] AsBase58(string Input, bool Checksum = false, string Digits = Glazer)
        {
			var BigInt = Input.AsBigInteger(Digits);
            var Result = byte.MinValue
				.Repeat(Input.Leadings(Digits[0]))
				.Concat(BigInt.MakeByteArray(true, true))
				.ToArray();

			if (Checksum)
            {
                var Data = new ArraySegment<byte>(Result, 0, Result.Length - SIZE_CHKSUM);
                var CSum = new ArraySegment<byte>(Result, Data.Count, SIZE_CHKSUM);
				var Hash = Hashes.Default.Hash("DSHA256", Data).Value;

				if (!new Span<byte>(Hash, 0, SIZE_CHKSUM).SequenceEqual(CSum))
                    throw new ArgumentException("the Input stream has been corrupted.");

                return Data.ToArray();
            }

            return Result;
        }
	}
}
