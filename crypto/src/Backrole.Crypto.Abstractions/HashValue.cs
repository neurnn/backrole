using Backrole.Crypto.Abstractions.Internals;
using System;
using System.Linq;

namespace Backrole.Crypto.Abstractions
{
    /// <summary>
    /// Hash Value.
    /// </summary>
    public struct HashValue : IEquatable<HashValue>, IAlgorithmParameter
    {
        private static readonly string EMPTY_NAME = "";
        private static readonly byte[] EMPTY_BYTES = new byte[0];

        /// <summary>
        /// Empty Value of the <see cref="HashValue"/>.
        /// </summary>
        public static readonly HashValue Empty = new HashValue();

        /// <summary>
        /// Initialize a new <see cref="HashValue"/>.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public HashValue(string Name, byte[] Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        /// <summary>
        /// Initialize a new <see cref="HashValue"/>.
        /// </summary>
        /// <param name="Input"></param>
        public HashValue(string Input)
        {
            var Temp = Parse(Input);

            Name = Temp.Name;
            Value = Temp.Value;
        }

        /* Compares two hash values. */
        public static bool operator ==(HashValue Left, HashValue Right) => Left.Equals(Right);
        public static bool operator !=(HashValue Left, HashValue Right) => !Left.Equals(Right);

        /// <summary>
        /// Try to parse the <paramref name="Input"/> to <paramref name="Output"/>.
        /// </summary>
        /// <returns></returns>
        public static bool TryParse(string Input, out HashValue Output)
        {
            var Collon = (Input ?? EMPTY_NAME).IndexOf(':');
            if (Collon > 0)
            {
                var Hex = Input.Substring(Collon + 1).ToLower();
                if ((Hex.Length % 2) == 0 && Hex.IsHexString())
                {
                    var Name = Input.Substring(0, Collon);
                    var Value = new byte[Hex.Length / 2];

                    for(var i = 0; i < Value.Length; ++i)
                    {
                        var H = Hex[i * 2 + 0].HexVal();
                        var L = Hex[i * 2 + 1].HexVal();
                        Value[i] = (byte)((H << 4) | L);
                    }

                    Output = new HashValue(Name, Value);
                    return true;
                }
            }

            Output = default;
            return false;
        }

        /// <summary>
        /// Parse the <paramref name="Input"/> to <paramref name="Output"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static HashValue Parse(string Input)
        {
            if (TryParse(Input, out var Value)) return Value;
            throw new FormatException($"{Input} isn't a hash value string.");
        }

        /// <summary>
        /// Test whether the hash value is valid or not.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Value != null && Value.Length > 0;

        /// <summary>
        /// Name of the hash algorithm.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Hash Value.
        /// </summary>
        public byte[] Value { get; }

        /// <inheritdoc/>
        public bool Equals(HashValue Other)
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
            if (Object is HashValue Other)
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

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsValid)
            {
                return string.Join(':', Name, BitConverter.ToString(Value).Replace("-", "")).ToLower();
            }

            return EMPTY_NAME;
        }
    }
}
