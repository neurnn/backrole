using Backrole.Crypto.Abstractions;
using Backrole.Crypto.Internals;
using System;
using System.Linq;

namespace Backrole.Crypto
{
    public struct SignKeyPair : IEquatable<SignKeyPair>, IAlgorithmParameter
    {
        private static readonly string EMPTY_NAME = "";
        private static readonly byte[] EMPTY_BYTES = new byte[0];

        /// <summary>
        /// Empty Value of the <see cref="SignKeyPair"/>.
        /// </summary>
        public static readonly SignKeyPair Empty = new SignKeyPair();

        /* Hidden Key Pair. */
        private byte[] m_Pvt, m_Pub;

        /// <summary>
        /// Initialize a new <see cref="SignKeyPair"/>.
        /// </summary>
        /// <param name="Pvt"></param>
        /// <param name="Pub"></param>
        public SignKeyPair(SignPrivateKey Pvt, SignPublicKey Pub)
        {
            if (!Pvt.IsValid || !Pub.IsValid)
                throw new ArgumentException("Invalid Key Pair specified.");

            if (!Pvt.Name.Equals(Pub.Name, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Algorithm mismatch between Pvt and Pub.");

            Name = Pvt.Name; Value = Binary.Pack(m_Pvt = Pvt.Value, m_Pub = Pub.Value);
        }

        /// <summary>
        /// Initialize a new <see cref="SignKeyPair"/>.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Pvt"></param>
        /// <param name="Pub"></param>
        public SignKeyPair(string Name, byte[] Pvt, byte[] Pub)
        {
            this.Name = Name;
            Value = Binary.Pack(Pvt, Pub);

            m_Pvt = Pvt;
            m_Pub = Pub;
        }

        /// <summary>
        /// Initialize a new <see cref="SignKeyPair"/>.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="PackedBytes"></param>
        public SignKeyPair(string Name, byte[] PackedBytes)
        {
            var Unpack = Binary.Unpack(Value = PackedBytes);
            this.Name = Name;

            m_Pvt = Unpack.Front;
            m_Pub = Unpack.Back;
        }

        /// <summary>
        /// Initialize a new <see cref="SignKeyPair"/>.
        /// </summary>
        /// <param name="Input"></param>
        public SignKeyPair(string Input)
        {
            var Temp = Parse(Input);

            Name = Temp.Name;
            Value = Temp.Value;

            m_Pvt = Temp.m_Pvt;
            m_Pub = Temp.m_Pub;
        }

        /* Compares two values. */
        public static bool operator ==(SignKeyPair Left, SignKeyPair Right) => Left.Equals(Right);
        public static bool operator !=(SignKeyPair Left, SignKeyPair Right) => !Left.Equals(Right);

        /// <summary>
        /// Try to parse the <paramref name="Input"/> to <paramref name="Output"/>.
        /// </summary>
        /// <returns></returns>
        public static bool TryParse(string Input, out SignKeyPair Output)
        {
            var Collon = (Input ?? EMPTY_NAME).IndexOf(':');
            if (Collon > 0)
            {
                var Hex = Input.Substring(Collon + 1).ToLower();
                if ((Hex.Length % 2) == 0 && Hex.IsHexString())
                {
                    var Name = Input.Substring(0, Collon);
                    var Value = new byte[Hex.Length / 2];

                    for (var i = 0; i < Value.Length; ++i)
                    {
                        var H = Hex[i * 2 + 0].HexVal();
                        var L = Hex[i * 2 + 1].HexVal();
                        Value[i] = (byte)((H << 4) | L);
                    }

                    Output = new SignKeyPair(Name, Value);
                    return true;
                }
            }

            Output = default;
            return false;
        }

        /// <summary>
        /// Parse the <paramref name="Input"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static SignKeyPair Parse(string Input)
        {
            if (TryParse(Input, out var Value)) return Value;
            throw new FormatException($"{Input} isn't value string.");
        }

        /// <summary>
        /// Indicates whether the value is valid or not.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Value != null;

        /// <summary>
        /// Sign the input and generate its signature.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Provider"></param>
        /// <returns></returns>
        public SignValue Sign(ArraySegment<byte> Input, ISignAlgorithmProvider Provider = null)
            => (Provider ?? Signs.Default).Sign(Name, PrivateKey, Input);

        /// <summary>
        /// Sign the input and generate its signature as seal.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Provider"></param>
        /// <returns></returns>
        public SignSealValue SignSeal(ArraySegment<byte> Input, ISignAlgorithmProvider Provider = null)
            => (Provider ?? Signs.Default).SignSeal(Name, this, Input);

        /// <summary>
        /// Name of the algorithm.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Packed Bytes.
        /// </summary>
        public byte[] Value { get; }

        /// <summary>
        /// Private Key.
        /// </summary>
        public SignPrivateKey PrivateKey => new SignPrivateKey(Name, m_Pvt);

        /// <summary>
        /// Public Key.
        /// </summary>
        public SignPublicKey PublicKey => new SignPublicKey(Name, m_Pub);

        /// <inheritdoc/>
        public bool Equals(SignKeyPair Other)
        {
            if (IsValid == Other.IsValid && IsValid)
            {
                if (!Name.Equals(Other.Name, StringComparison.OrdinalIgnoreCase))
                    return false;

                ReadOnlySpan<byte> ValueSpan = Value;
                return ValueSpan.SequenceEqual(Other.Value);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object Object)
        {
            if (Object is SignKeyPair Other)
                return Equals(Other);

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (IsValid)
                return HashCode.Combine(Name, Value);

            return HashCode.Combine(EMPTY_NAME, EMPTY_BYTES);
        }

        /// <summary>
        /// Returns the string expression of the instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IsValid)
            {
                return string.Join(':', Name, Value.ToHex()).ToLower();
            }

            return EMPTY_NAME;
        }
    }
}
