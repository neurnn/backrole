using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Crypto.Abstractions
{
    public static class SignHelpers
    {
        /// <summary>
        /// Test whether the public key is compatible with the specified algorithm or not.
        /// </summary>
        /// <param name="Pub"></param>
        /// <param name="Algorithm"></param>
        /// <returns></returns>
        public static bool IsSuitable(this SignPublicKey Pub, ISignAlgorithm Algorithm)
        {
            if (!Pub.IsValid || !Pub.Name.Equals(Algorithm.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            if (Pub.Value.Length != Algorithm.SizeOfPublicKey)
                return false;

            return true;
        }

        /// <summary>
        /// Test whether the private key is compatible with the specified algorithm or not.
        /// </summary>
        /// <param name="Pvt"></param>
        /// <param name="Algorithm"></param>
        /// <returns></returns>
        public static bool IsSuitable(this SignPrivateKey Pvt, ISignAlgorithm Algorithm)
        {
            if (!Pvt.IsValid || !Pvt.Name.Equals(Algorithm.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            if (Pvt.Value.Length != Algorithm.SizeOfPrivateKey)
                return false;

            return true;
        }

        /// <summary>
        /// Test whether the key pair is compatible with the specified algorithm or not.
        /// </summary>
        /// <param name="KeyPair"></param>
        /// <param name="Algorithm"></param>
        /// <returns></returns>
        public static bool IsSuitable(this SignKeyPair KeyPair, ISignAlgorithm Algorithm)
        {
            if (KeyPair.IsValid && KeyPair.Name.Equals(Algorithm.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (KeyPair.PublicKey.Value.Length != Algorithm.SizeOfPublicKey)
                    return false;

                if (KeyPair.PrivateKey.Value.Length != Algorithm.SizeOfPrivateKey)
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Throw <see cref="ArgumentException"/> if the public key is incompatible.
        /// </summary>
        /// <param name="Pub"></param>
        /// <param name="Algorithm"></param>
        public static void ThrowIfIncompatible(this SignPublicKey Pub, ISignAlgorithm Algorithm)
        {
            if (!Pub.IsValid || !Pub.Name.Equals(Algorithm.Name, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"the public key is incompatible with {Algorithm.Name} algorithm.");

            if (Pub.Value.Length != Algorithm.SizeOfPublicKey)
                throw new ArgumentException($"the public key is corrupted.");
        }

        /// <summary>
        /// Throw <see cref="ArgumentException"/> if the private key is incompatible.
        /// </summary>
        /// <param name="Pvt"></param>
        /// <param name="Algorithm"></param>
        public static void ThrowIfIncompatible(this SignPrivateKey Pvt, ISignAlgorithm Algorithm)
        {
            if (!Pvt.IsValid || !Pvt.Name.Equals(Algorithm.Name, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"the private key is incompatible with {Algorithm.Name} algorithm.");

            if (Pvt.Value.Length != Algorithm.SizeOfPrivateKey)
                throw new ArgumentException($"the private key is corrupted.");
        }


        /// <summary>
        /// Throw <see cref="ArgumentException"/> if the key pair is incompatible.
        /// </summary>
        /// <param name="KeyPair"></param>
        /// <param name="Algorithm"></param>
        public static void ThrowIfIncompatible(this SignKeyPair KeyPair, ISignAlgorithm Algorithm)
        {
            if (!KeyPair.IsValid || !KeyPair.Name.Equals(Algorithm.Name, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"the key pair is incompatible with {Algorithm.Name} algorithm.");

            KeyPair.PrivateKey.ThrowIfIncompatible(Algorithm);
            KeyPair.PublicKey.ThrowIfIncompatible(Algorithm);
        }
    }
}
