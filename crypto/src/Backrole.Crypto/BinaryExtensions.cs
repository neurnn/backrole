using Backrole.Crypto.Abstractions;
using System.IO;
using System.Text;

namespace Backrole.Crypto
{
    public static class BinaryExtensions
    {
        /// <summary>
        /// Write the <see cref="IAlgorithmParameter"/> to the <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="Writer"></param>
        /// <param name="Hash"></param>
        public static void Write(this BinaryWriter Writer, IAlgorithmParameter Hash)
        {
            if (Hash.IsValid)
            {
                var Name = Encoding.ASCII.GetBytes(Hash.Name);

                Writer.Write7BitEncodedInt(Name.Length);
                Writer.Write7BitEncodedInt(Hash.Value.Length);

                Writer.Write(Name);
                Writer.Write(Hash.Value);
                return;
            }

            Writer.Write7BitEncodedInt(0);
            return;
        }

        /// <summary>
        /// Read <see cref="HashValue"/> from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        public static HashValue ReadHashValue(this BinaryReader Reader)
        {
            var LenName = Reader.Read7BitEncodedInt();
            if (LenName > 0)
            {
                var LenHash = Reader.Read7BitEncodedInt();

                var Name = Encoding.ASCII.GetString(Reader.ReadBytes(LenName));
                var Hash = Reader.ReadBytes(LenHash);

                return new HashValue(Name, Hash);
            }

            return HashValue.Empty;
        }

        /// <summary>
        /// Read <see cref="SignValue"/> from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        public static SignValue ReadSignValue(this BinaryReader Reader)
        {
            var LenName = Reader.Read7BitEncodedInt();
            if (LenName > 0)
            {
                var LenHash = Reader.Read7BitEncodedInt();

                var Name = Encoding.ASCII.GetString(Reader.ReadBytes(LenName));
                var Hash = Reader.ReadBytes(LenHash);

                return new SignValue(Name, Hash);
            }

            return SignValue.Empty;
        }

        /// <summary>
        /// Read <see cref="SignPrivateKey"/> from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        public static SignPrivateKey ReadSignPrivateKey(this BinaryReader Reader)
        {
            var LenName = Reader.Read7BitEncodedInt();
            if (LenName > 0)
            {
                var LenHash = Reader.Read7BitEncodedInt();

                var Name = Encoding.ASCII.GetString(Reader.ReadBytes(LenName));
                var Hash = Reader.ReadBytes(LenHash);

                return new SignPrivateKey(Name, Hash);
            }

            return SignPrivateKey.Empty;
        }

        /// <summary>
        /// Read <see cref="SignPublicKey"/> from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        public static SignPublicKey ReadSignPublicKey(this BinaryReader Reader)
        {
            var LenName = Reader.Read7BitEncodedInt();
            if (LenName > 0)
            {
                var LenHash = Reader.Read7BitEncodedInt();

                var Name = Encoding.ASCII.GetString(Reader.ReadBytes(LenName));
                var Hash = Reader.ReadBytes(LenHash);

                return new SignPublicKey(Name, Hash);
            }

            return SignPublicKey.Empty;
        }

        /// <summary>
        /// Read <see cref="SignKeyPair"/> from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        public static SignKeyPair ReadSignKeyPair(this BinaryReader Reader)
        {
            var LenName = Reader.Read7BitEncodedInt();
            if (LenName > 0)
            {
                var LenHash = Reader.Read7BitEncodedInt();

                var Name = Encoding.ASCII.GetString(Reader.ReadBytes(LenName));
                var Hash = Reader.ReadBytes(LenHash);

                return new SignKeyPair(Name, Hash);
            }

            return SignKeyPair.Empty;
        }

        /// <summary>
        /// Read <see cref="SignSealValue"/> from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        public static SignSealValue ReadSignSealValue(this BinaryReader Reader)
        {
            var LenName = Reader.Read7BitEncodedInt();
            if (LenName > 0)
            {
                var LenHash = Reader.Read7BitEncodedInt();

                var Name = Encoding.ASCII.GetString(Reader.ReadBytes(LenName));
                var Hash = Reader.ReadBytes(LenHash);

                return new SignSealValue(Name, Hash);
            }

            return SignSealValue.Empty;
        }
    }
}
