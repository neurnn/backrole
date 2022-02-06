using Backrole.Crypto.Abstractions;
using System;
using NSECP256K1 = Secp256k1Net.Secp256k1;

namespace Backrole.Crypto.Algorithms
{
    public class Secp256k1 : ISignAlgorithm
    {
        /// <inheritdoc/>
        public int SizeOfPrivateKey => NSECP256K1.PRIVKEY_LENGTH;

        /// <inheritdoc/>
        public int SizeOfPublicKey => NSECP256K1.PUBKEY_LENGTH;

        /// <inheritdoc/>
        public int SizeOfSignature => NSECP256K1.SIGNATURE_LENGTH;

        /// <inheritdoc/>
        public string Name => "SECP256K1";

        /// <inheritdoc/>
        public SignKeyPair MakeKeyPair(bool TestDeep = false)
        {
            using(var Secp = new NSECP256K1())
            {
                var Pub = new byte[NSECP256K1.PUBKEY_LENGTH];
                while (true)
                {
                    var Pvt = Rng.Make(NSECP256K1.PRIVKEY_LENGTH, X => Secp.SecretKeyVerify(X));
                    if (!Secp.PublicKeyCreate(Pub, Pvt))
                        continue;

                    var Key = new SignKeyPair(Name, Pvt, Pub);
                    if (TestDeep && !TestKeyPair(Key))
                        continue;

                    return Key;
                }
            }
        }

        /// <inheritdoc/>
        public SignKeyPair MakeKeyPair(SignPrivateKey Pvt, bool TestDeep = false)
        {
            Pvt.ThrowIfIncompatible(this);

            using (var Secp = new NSECP256K1())
            {
                var Pub = new byte[NSECP256K1.PUBKEY_LENGTH];
                if (!Secp.PublicKeyCreate(Pub, Pvt.Value))
                    SignPrivateKey.Empty.ThrowIfIncompatible(this);

                var Key = new SignKeyPair(Name, Pvt.Value, Pub);
                if (TestDeep && !TestKeyPair(Key))
                    SignPrivateKey.Empty.ThrowIfIncompatible(this);

                return Key;
            }
        }

        /// <inheritdoc/>
        public bool TestKeyPair(SignKeyPair KeyPair)
        {
            if (!KeyPair.IsSuitable(this))
                return false;

            using (var Secp = new NSECP256K1())
            {
                var Hash = Hashes.Default.Hash("SHA256", Rng.Make(256));
                var Sign = new byte[NSECP256K1.SIGNATURE_LENGTH];

                if (!Secp.Sign(Sign, Hash.Value, KeyPair.PrivateKey.Value))
                    return false;

                return Secp.Verify(Sign, Hash.Value, KeyPair.PublicKey.Value);
            }
        }

        /// <inheritdoc/>
        public SignValue Sign(SignPrivateKey Pvt, ArraySegment<byte> Input)
        {
            Pvt.ThrowIfIncompatible(this);

            using (var Secp = new NSECP256K1())
            {
                var Hash = Hashes.Default.Hash("SHA256", Input);
                var Sign = new byte[NSECP256K1.SIGNATURE_LENGTH];

                if (!Secp.Sign(Sign, Hash.Value, Pvt.Value))
                    throw new ArgumentException("No signature calculated.");

                return new SignValue(Name, Sign);
            }
        }

        /// <inheritdoc/>
        public bool Verify(SignPublicKey Pub, SignValue Sign, ArraySegment<byte> Input)
        {
            Pub.ThrowIfIncompatible(this);

            using (var Secp = new NSECP256K1())
            {
                var Hash = Hashes.Default.Hash("SHA256", Input);
                return Secp.Verify(Sign.Value, Hash.Value, Pub.Value);
            }
        }
    }
}
