using System;
using System.Security.Cryptography;

namespace Backrole.Crypto
{
    public static class Rng
    {
        /// <summary>
        /// Make the cryptographic random bytes.
        /// </summary>
        /// <param name="Length"></param>
        /// <param name="NonZero"></param>
        /// <returns></returns>
        public static byte[] Make(int Length, bool NonZero = false) => Fill(new byte[Length], NonZero);

        /// <summary>
        /// Make the cryptographic random bytes with the external validator delegate.
        /// </summary>
        /// <param name="Length"></param>
        /// <param name="Validator"></param>
        /// <param name="NonZero"></param>
        /// <returns></returns>
        public static byte[] Make(int Length, Func<byte[], bool> Validator, bool NonZero = false) => Fill(new byte[Length], Validator, NonZero);

        /// <summary>
        /// Fill the cryptographic random bytes to the buffer.
        /// </summary>
        /// <param name="Where"></param>
        /// <returns></returns>
        public static byte[] Fill(byte[] Where, bool NonZero = false) => Fill(Where, null, NonZero);

        /// <summary>
        /// Fill the cryptographic random bytes to the buffer with the external validator delegate.
        /// </summary>
        /// <param name="Where"></param>
        /// <param name="Validator"></param>
        /// <param name="NonZero"></param>
        /// <returns></returns>
        public static byte[] Fill(byte[] Where, Func<byte[], bool> Validator, bool NonZero = false)
        {
            using (var Rng = RandomNumberGenerator.Create())
            {
                while (true)
                {
                    if (NonZero)
                        Rng.GetNonZeroBytes(Where);

                    else
                        Rng.GetBytes(Where);

                    if (Validator is null || Validator(Where))
                        break;
                }

                return Where;
            }
        }
    }
}
