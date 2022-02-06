using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Crypto.Internals
{
    internal static class RsaHelpers
    {
        private static readonly BigInteger ZERO = 0;
        private static readonly BigInteger ONE = 1;
        private static readonly BigInteger TWO = 2;
        
        /// <summary>
        /// Recover the <see cref="RSAParameters"/> from Modulus, Exponent, D point.
        /// </summary>
        /// <param name="ModulusBytes"></param>
        /// <param name="ExponentBytes"></param>
        /// <param name="DBytes"></param>
        /// <returns></returns>
        public static RSAParameters Recover(byte[] ModulusBytes, byte[] ExponentBytes, byte[] DBytes)
        {
            BigInteger 
                M = ModulusBytes.ToBigInteger(), 
                E = ExponentBytes.ToBigInteger(),
                D = DBytes.ToBigInteger();

            var k = D * E - 1;
            if (!k.IsEven)
            {
                throw new InvalidOperationException("d*e - 1 is odd");
            }

            var t = ONE;
            var r = k / TWO;

            while (r.IsEven)
            {
                t++;
                r /= TWO;
            }

            Recover(M, t, r, out var Buffer, out var y);

            var P = BigInteger.GreatestCommonDivisor(y - ONE, M);
            var Q = M / P;
            var DP = D % (P - ONE);
            var DQ = D % (Q - ONE);
            var InvQ = ModInv(Q, P);

            var ModLen = Buffer.Length;
            var ModHalfLen = (ModLen + 1) / 2;

            return new RSAParameters
            {
                Modulus = GetBytes(M, ModLen),
                Exponent = GetBytes(E, -1),
                D = GetBytes(D, ModLen),
                P = GetBytes(P, ModHalfLen),
                Q = GetBytes(Q, ModHalfLen),
                DP = GetBytes(DP, ModHalfLen),
                DQ = GetBytes(DQ, ModHalfLen),
                InverseQ = GetBytes(InvQ, ModHalfLen),
            };

        }

        private static void Recover(BigInteger M, BigInteger t, BigInteger r, out byte[] Buffer, out BigInteger y)
        {
            Buffer = M.ToByteArray();
            if (Buffer[Buffer.Length - 1] == 0)
                Buffer = new byte[Buffer.Length - 1];

            var nM1 = M - ONE;
            var Crack = false;
            y = ZERO;

            for (int i = 0; i < 100 && !Crack; i++)
            {
                var g = ZERO;
                do
                {
                    g = Rng.Fill(Buffer).ToBigInteger();
                }
                while (g >= M);

                y = BigInteger.ModPow(g, r, M);

                if (y.IsOne || y == nM1)
                {
                    i--;
                    continue;
                }

                for (var j = ONE; j < t; j++)
                {
                    var x = BigInteger.ModPow(y, TWO, M);

                    if (x.IsOne)
                    {
                        Crack = true;
                        break;
                    }

                    if (x == nM1)
                    {
                        break;
                    }

                    y = x;
                }
            }

            if (!Crack)
            {
                throw new InvalidOperationException("Prime factors not found");
            }
        }

        private static byte[] GetBytes(BigInteger Value, int Size)
        {
            var Bytes = Value.ToByteArray();
            if (Size == -1)
                Size = Bytes.Length;

            if (Bytes.Length > Size + 1)
                throw new InvalidOperationException($"Cannot squeeze value {Value} to {Size} bytes from {Bytes.Length}.");

            if (Bytes.Length == Size + 1 && Bytes[Bytes.Length - 1] != 0)
                throw new InvalidOperationException($"Cannot squeeze value {Value} to {Size} bytes from {Bytes.Length}.");
            
            Array.Resize(ref Bytes, Size);
            Array.Reverse(Bytes);
            return Bytes;
        }

        private static BigInteger ModInv(BigInteger E, BigInteger N)
        {
            BigInteger R = N, NewR = E, T = ZERO, NewT = ONE;
            while (NewR != 0)
            {
                BigInteger Q = R / NewR, X = T, Y = R;
                T = NewT; NewT = X - Q * NewT;
                R = NewR; NewR = Y - Q * NewR;
            }

            if (T < 0)
                return T + N;

            return T;
        }
    }
}
