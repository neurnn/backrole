using System;

namespace Backrole.Crypto.Abstractions
{
    /// <summary>
    /// Sign or verify the signature.
    /// </summary>
    public interface ISignAlgorithm : IAlgorithm
    {
        /// <summary>
        /// Size of the Private Key in bytes.
        /// </summary>
        int SizeOfPrivateKey { get; }

        /// <summary>
        /// Size of the Public Key in bytes.
        /// </summary>
        int SizeOfPublicKey { get; }

        /// <summary>
        /// Size of the Signature in bytes.
        /// </summary>
        int SizeOfSignature { get; }

        /// <summary>
        /// Makes a new <see cref="SignKeyPair"/>.
        /// </summary>
        /// <param name="TestDeep"></param>
        /// <returns></returns>
        SignKeyPair MakeKeyPair(bool TestDeep = false);

        /// <summary>
        /// Makes a new <see cref="SignKeyPair"/> from <see cref="SignPrivateKey"/>.
        /// </summary>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pvt"></param>
        /// <param name="TestDeep"></param>
        /// <returns></returns>
        SignKeyPair MakeKeyPair(SignPrivateKey Pvt, bool TestDeep = false);

        /// <summary>
        /// Test the <see cref="SignKeyPair"/> once.
        /// </summary>
        /// <param name="KeyPair"></param>
        /// <returns></returns>
        bool TestKeyPair(SignKeyPair KeyPair);

        /// <summary>
        /// Sign the input bytes using <see cref="SignPrivateKey"/>.
        /// </summary>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pvt"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        SignValue Sign(SignPrivateKey Pvt, ArraySegment<byte> Input);

        /// <summary>
        /// Verify the input bytes and the sign value using the <see cref="SignPublicKey"/>.
        /// </summary>
        /// <exception cref="ArgumentException">if the input key is not valid.</exception>
        /// <param name="Pub"></param>
        /// <param name="Sign"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        bool Verify(SignPublicKey Pub, SignValue Sign, ArraySegment<byte> Input);
    }
}
