using Backrole.Crypto.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Crypto.Internals
{
    public abstract class RsaWrapper : ISignAlgorithm
    {
        private int m_KeySize;

        /// <inheritdoc/>
        public RsaWrapper(int KeySize) => m_KeySize = KeySize;

        /// <inheritdoc/>
        public int SizeOfPrivateKey => (m_KeySize / 8) * 2 + 3;

        /// <inheritdoc/>
        public int SizeOfPublicKey => (m_KeySize / 8) + 3;

        /// <inheritdoc/>
        public int SizeOfSignature => m_KeySize / 8;

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public SignKeyPair MakeKeyPair(bool TestDeep = false)
        {
            using var Rsa = RSA.Create(m_KeySize);
            var Params = Rsa.ExportParameters(true);

            var Pvt = Params.Modulus.Concat(Params.Exponent).Concat(Params.D);
            var Pub = Params.Modulus.Concat(Params.Exponent);
            return new SignKeyPair(Name, Pvt, Pub);
        }

        /// <inheritdoc/>
        public SignKeyPair MakeKeyPair(SignPrivateKey Pvt, bool TestDeep = false)
        {
            Pvt.ThrowIfIncompatible(this);

            var Modulus = Pvt.Value.Subset(0, m_KeySize / 8);
            var Exponent = Pvt.Value.Subset(m_KeySize / 8, 3);
            var D = Pvt.Value.Subset((m_KeySize / 8) + 3);

            return new SignKeyPair(Name, Pvt.Value, Modulus.Concat(Exponent));
        }

        /// <inheritdoc/>
        public bool TestKeyPair(SignKeyPair KeyPair)
        {
            KeyPair.ThrowIfIncompatible(this);

            var Temp = Rng.Make(2048);
            var Data = Sign(KeyPair.PrivateKey, Temp);
            return Verify(KeyPair.PublicKey, Data, Temp);
        }

        /// <inheritdoc/>
        public SignValue Sign(SignPrivateKey Pvt, ArraySegment<byte> Input)
        {
            Pvt.ThrowIfIncompatible(this);
            using var Rsa = RSA.Create(m_KeySize);

            var Modulus = Pvt.Value.Subset(0, m_KeySize / 8);
            var Exponent = Pvt.Value.Subset(m_KeySize / 8, 3);
            var D = Pvt.Value.Subset((m_KeySize / 8) + 3);

            Rsa.ImportParameters(RsaHelpers.Recover(Modulus, Exponent, D));

            var Sign = Rsa.SignData(Input.Array, Input.Offset, Input.Count,
                HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);

            return new SignValue(Name, Sign);
        }

        /// <inheritdoc/>
        public bool Verify(SignPublicKey Pub, SignValue Sign, ArraySegment<byte> Input)
        {
            Pub.ThrowIfIncompatible(this);
            using var Rsa = RSA.Create(m_KeySize);
            Rsa.ImportParameters(new RSAParameters
            {
                Modulus = Pub.Value.Subset(0, m_KeySize / 8),
                Exponent = Pub.Value.Subset(m_KeySize / 8, 3)
            });

            return Rsa.VerifyData(Input.Array, Input.Offset, Input.Count,
                Sign.Value, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }
    }
}
