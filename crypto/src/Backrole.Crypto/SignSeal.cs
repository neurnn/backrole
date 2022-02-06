using Backrole.Crypto.Abstractions;
using Backrole.Crypto.Internals;
using System;

namespace Backrole.Crypto
{
    /// <summary>
    /// Signature Seal.
    /// </summary>
    public struct SignSealValue : IEquatable<SignSealValue>, IAlgorithmParameter
    {
        private static readonly string EMPTY_NAME = "";
        private static readonly byte[] EMPTY_BYTES = new byte[0];

        /// <summary>
        /// Empty Value of the <see cref="SignSealValue"/>.
        /// </summary>
        public static readonly SignSealValue Empty = new SignSealValue();

        private byte[] m_Pub, m_Sign;

        /// <summary>
        /// Initialize a new <see cref="SignSealValue"/>.
        /// </summary>
        /// <param name="Pub"></param>
        /// <param name="Sign"></param>
        public SignSealValue(SignValue Sign, SignPublicKey Pub)
        {
            if (!Sign.IsValid || !Pub.IsValid)
                throw new ArgumentException("Invalid values specified.");

            if (!Sign.Name.Equals(Pub.Name, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Algorithm mismatch between Sign and Pub.");

            Name = Pub.Name; Value = Binary.Pack(m_Pub = Pub.Value, m_Sign = Sign.Value);
        }

        /// <summary>
        /// Initialize a new <see cref="SignSealValue"/>.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Pub"></param>
        /// <param name="Sign"></param>
        public SignSealValue(string Name, byte[] Pub, byte[] Sign)
        {
            Value = Binary.Pack(m_Pub = Pub, m_Sign = Sign);
            this.Name = Name;
        }

        /// <summary>
        /// Initialize a new <see cref="SignSealValue"/>.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="PackedBytes"></param>
        public SignSealValue(string Name, byte[] PackedBytes)
        {
            var Unpack = Binary.Unpack(Value = PackedBytes);
            this.Name = Name;

            m_Pub = Unpack.Front;
            m_Sign = Unpack.Back;
        }

        /* Compares two values. */
        public static bool operator ==(SignSealValue Left, SignSealValue Right) => Left.Equals(Right);
        public static bool operator !=(SignSealValue Left, SignSealValue Right) => !Left.Equals(Right);

        /// <summary>
        /// Try to parse the <paramref name="Input"/> to <paramref name="Output"/>.
        /// </summary>
        /// <returns></returns>
        public static bool TryParse(string Input, out SignSealValue Output)
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

                    Output = new SignSealValue(Name, Value);
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
        public static SignSealValue Parse(string Input)
        {
            if (TryParse(Input, out var Value)) return Value;
            throw new FormatException($"{Input} isn't value string.");
        }

        /// <summary>
        /// Indicates whether the value is valid or not.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Value != null;

        /// <summary>
        /// Verify the Input bytes using simple hash comparison.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Provider"></param>
        /// <returns></returns>
        public bool Verify(ArraySegment<byte> Input, ISignAlgorithmProvider Provider = null)
            => IsValid && (Provider ?? Signs.Default).Verify(Name, PublicKey, Signature, Input);

        /// <summary>
        /// Name of the algorithm.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Packed Bytes.
        /// </summary>
        public byte[] Value { get; }

        /// <summary>
        /// Public Key.
        /// </summary>
        public SignValue Signature => new SignValue(Name, m_Sign);

        /// <summary>
        /// Public Key.
        /// </summary>
        public SignPublicKey PublicKey => new SignPublicKey(Name, m_Pub);

        /// <inheritdoc/>
        public bool Equals(SignSealValue Other)
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
            if (Object is SignSealValue Other)
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
