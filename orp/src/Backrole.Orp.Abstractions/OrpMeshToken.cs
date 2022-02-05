using Secp256k1Net;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Backrole.Orp.Abstractions
{
    /// <summary>
    /// Mesh Token.
    /// </summary>
    public class OrpMeshToken : IOrpPackable, IOrpUnpackable, IEquatable<OrpMeshToken>
    {
        private static readonly byte[] EMPTY_BYTES = new byte[0];

        private const int SIZE_SHA256_HASH = 32;
        private const int SIZE_PRIVATE_KEY = 32;
        private const int SIZE_PUBLIC_KEY = 64;
        private const int SIZE_SIGNATRUE = 64;

        /// <summary>
        /// Initialize a new <see cref="OrpMeshToken"/> instance.
        /// </summary>
        public OrpMeshToken()
        {
            Identity = EMPTY_BYTES;
            PublicKey = EMPTY_BYTES;
        }

        /// <summary>
        /// Initialize a new <see cref="OrpMeshToken"/> instance.
        /// </summary>
        /// <param name="Identity"></param>
        /// <param name="PublicKey"></param>
        public OrpMeshToken(byte[] Identity, byte[] PublicKey)
        {
            this.Identity = Identity;
            this.PublicKey = PublicKey;
        }

        /// <summary>
        /// Initialize a new <see cref="OrpMeshToken"/> instance.
        /// </summary>
        /// <param name="Identity"></param>
        /// <param name="PublicKey"></param>
        public OrpMeshToken(byte[] Identity, byte[] PublicKey, byte[] PrivateKey)
        {
            this.Identity = Identity;
            this.PublicKey = PublicKey;
            this.PrivateKey = PrivateKey;
        }

        /// <summary>
        /// Create a new <see cref="Secp256k1"/> key.
        /// </summary>
        /// <returns></returns>
        private static byte[] NewPrivateKey()
        {
            var PrivateKey = new byte[SIZE_PRIVATE_KEY];
            using (var SECP = new Secp256k1())
            {
                using (RandomNumberGenerator RNG = RandomNumberGenerator.Create())
                {
                    do { RNG.GetBytes(PrivateKey); }
                    while (!SECP.SecretKeyVerify(PrivateKey));
                }
            }

            return PrivateKey;
        }

        /// <summary>
        /// Calculate a public key from <see cref="Secp256k1"/> key.
        /// </summary>
        /// <param name="PrivateKey"></param>
        /// <returns></returns>
        private static byte[] NewPublicKey(byte[] PrivateKey)
        {
            var PublicKey = new byte[SIZE_PUBLIC_KEY];
            using (var SECP = new Secp256k1())
            {
                if (!SECP.PublicKeyCreate(PublicKey, PrivateKey))
                    throw new ArgumentException(nameof(PrivateKey));
            }

            return PublicKey;
        }

        /// <summary>
        /// Hash Input bytes.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        private static byte[] HashSHA256(byte[] Input)
        {
            using (var Sha256 = SHA256.Create())
                return Sha256.ComputeHash(Input);
        }

        /// <summary>
        /// Generate a new <see cref="OrpMeshToken"/>.
        /// </summary>
        /// <returns></returns>
        public static OrpMeshToken New()
        {
            var Pvt = NewPrivateKey();
            var Pub = NewPublicKey(Pvt);

            return new OrpMeshToken(HashSHA256(Pvt.Concat(Pub).ToArray()), Pub, Pvt);
        }

        /// <summary>
        /// Indicates whether the token is valid or not.
        /// </summary>
        public bool IsValid 
            =>  Identity != null &&                    PublicKey != null && 
                Identity.Length == SIZE_SHA256_HASH && PublicKey.Length == SIZE_PUBLIC_KEY;

        /// <summary>
        /// Indicate whether the token has private key or not.
        /// </summary>
        public bool HasPrivateKey
            => PrivateKey != null && PrivateKey.Length == SIZE_PRIVATE_KEY;

        /// <summary>
        /// Identity of the peer.
        /// </summary>
        public byte[] Identity { get; private set; }

        /// <summary>
        /// Public Key of the peer.
        /// </summary>
        public byte[] PublicKey { get; private set; }

        /// <summary>
        /// Private Key of the peer. (if the token from remote, this will be null)
        /// </summary>
        public byte[] PrivateKey { get; }

        /// <summary>
        /// Verify the payload is sent from the specified peer or not.
        /// </summary>
        /// <param name="Signature"></param>
        /// <param name="Payload"></param>
        /// <returns></returns>
        public bool Verify(byte[] Signature, byte[] Payload)
        {
            if (PublicKey is null)
                return false;

            var Hash = HashSHA256(Payload);
            using(var Secp = new Secp256k1())
                return Secp.Verify(Signature, Hash, PublicKey);
        }

        /// <summary>
        /// Sign the payload using the private key.
        /// </summary>
        /// <param name="Payload"></param>
        /// <param name="Signature"></param>
        /// <returns></returns>
        public bool Sign(byte[] Payload, out byte[] Signature)
        {
            if (PrivateKey is null || PrivateKey.Length != SIZE_PRIVATE_KEY)
            {
                Signature = EMPTY_BYTES;
                return false;
            }

            var Hash = HashSHA256(Payload);
            using (var Secp = new Secp256k1())
            {
                Signature = new byte[SIZE_SIGNATRUE];
                return Secp.Sign(Signature, Hash, PrivateKey);
            }
        }

        /// <inheritdoc/>
        public bool TryPack(BinaryWriter Output, IOrpReadOnlyOptions Options)
        {
            if (!IsValid)
                return false;

            try
            {
                Output.Write(Identity); // 32 bytes.
                Output.Write(PublicKey); // 64 bytes.
            }
            catch { return false; }
            return true;
        }

        /// <inheritdoc/>
        public bool TryUnpack(BinaryReader Input, IOrpReadOnlyOptions Options)
        {
            try
            {
                Identity = Input.ReadBytes(SIZE_SHA256_HASH);
                PublicKey = Input.ReadBytes(SIZE_PUBLIC_KEY);
            }
            catch
            {
                Identity = PublicKey = EMPTY_BYTES;
                return false;
            }

            return IsValid;
        }

        /// <summary>
        /// Test whether the other token equals with this or not.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public bool Equals(OrpMeshToken Other)
        {
            if (IsValid && Other != null)
            {
                if (ReferenceEquals(this, Other))
                    return true;

                Span<byte> Identity = this.Identity ?? EMPTY_BYTES;
                Span<byte> PublicKey = this.PublicKey ?? EMPTY_BYTES;

                return  Identity.SequenceEqual(Other.Identity ?? EMPTY_BYTES) &&
                        PublicKey.SequenceEqual(Other.PublicKey ?? EMPTY_BYTES);
            }

            return Other.IsValid;
        }

        /// <inheritdoc/>
        public override bool Equals(object Object)
        {
            if (Object is OrpMeshToken Token)
                return Equals(Token);

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (Identity != null && PublicKey != null)
                return HashCode.Combine(Identity, PublicKey);

            return HashCode.Combine(EMPTY_BYTES, EMPTY_BYTES);
        }
    }
}
