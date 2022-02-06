using Backrole.Crypto.Abstractions;
using System;

namespace Backrole.Crypto
{
    /// <summary>
    /// Provides `Verify` shortcut methods for <see cref="ISignAlgorithmProvider"/> instances.
    /// </summary>
    public static class SignExtensions
    {
        /// <summary>
        /// Sign the input bytes using <see cref="SignPrivateKey"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="ISignAlgorithmProvider"/>.</exception>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pvt"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static SignValue Sign(this ISignAlgorithmProvider Signs, SignPrivateKey Pvt, ArraySegment<byte> Input)
        {
            var Sign = Signs.Get(Pvt.Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Pvt.Name}");

            return Sign.Sign(Pvt, Input);
        }

        /// <summary>
        /// Sign the input bytes using <see cref="SignPrivateKey"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="ISignAlgorithmProvider"/>.</exception>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pvt"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static SignValue Sign(this ISignAlgorithmProvider Signs, string Name, SignPrivateKey Pvt, ArraySegment<byte> Input)
        {
            var Sign = Signs.Get(Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Name}");

            return Sign.Sign(Pvt, Input);
        }

        /// <summary>
        /// Sign the input bytes using <see cref="SignPrivateKey"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="ISignAlgorithmProvider"/>.</exception>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pvt"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static SignSealValue SignSeal(this ISignAlgorithmProvider Signs, SignKeyPair KeyPair, ArraySegment<byte> Input)
        {
            var Sign = Signs.Get(KeyPair.Name)
                ?? throw new NotSupportedException($"No algorithm supported: {KeyPair.Name}");

            return new SignSealValue(Sign.Sign(KeyPair.PrivateKey, Input), KeyPair.PublicKey);
        }

        /// <summary>
        /// Sign the input bytes using <see cref="SignPrivateKey"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="ISignAlgorithmProvider"/>.</exception>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pvt"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static SignSealValue SignSeal(this ISignAlgorithmProvider Signs, string Name, SignKeyPair KeyPair, ArraySegment<byte> Input)
        {
            var Sign = Signs.Get(Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Name}");

            return new SignSealValue(Sign.Sign(KeyPair.PrivateKey, Input), KeyPair.PublicKey);
        }

        /// <summary>
        /// Sign the input bytes using <see cref="SignPrivateKey"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="ISignAlgorithmProvider"/>.</exception>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pvt"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static SignSealValue SignSeal(this ISignAlgorithmProvider Signs, SignPrivateKey Pvt, ArraySegment<byte> Input)
        {
            var Sign = Signs.Get(Pvt.Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Pvt.Name}");
            
            return new SignSealValue(Sign.Sign(Pvt, Input), Sign.MakeKeyPair(Pvt).PublicKey);
        }

        /// <summary>
        /// Sign the input bytes using <see cref="SignPrivateKey"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="ISignAlgorithmProvider"/>.</exception>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pvt"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static SignSealValue SignSeal(this ISignAlgorithmProvider Signs, string Name, SignPrivateKey Pvt, ArraySegment<byte> Input)
        {
            var Sign = Signs.Get(Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Name}");

            return new SignSealValue(Sign.Sign(Pvt, Input), Sign.MakeKeyPair(Pvt).PublicKey);
        }


        /// <summary>
        /// Verify the input bytes and the sign value using the <see cref="SignPublicKey"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="ISignAlgorithmProvider"/>.</exception>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Signs"></param>
        /// <param name="Pub"></param>
        /// <param name="Value"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static bool Verify(this ISignAlgorithmProvider Signs, SignPublicKey Pub, SignValue Value, ArraySegment<byte> Input)
        {
            var Sign = Signs.Get(Pub.Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Pub.Name}");

            return Sign.Verify(Pub, Value, Input);
        }

        /// <summary>
        /// Verify the input bytes and the sign value using the <see cref="SignPublicKey"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">if no algorithm available from <see cref="ISignAlgorithmProvider"/>.</exception>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Signs"></param>
        /// <param name="Pub"></param>
        /// <param name="Value"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static bool Verify(this ISignAlgorithmProvider Signs, string Name, SignPublicKey Pub, SignValue Value, ArraySegment<byte> Input)
        {
            var Sign = Signs.Get(Name)
                ?? throw new NotSupportedException($"No algorithm supported: {Name}");

            return Sign.Verify(Pub, Value, Input);
        }
    }
}
