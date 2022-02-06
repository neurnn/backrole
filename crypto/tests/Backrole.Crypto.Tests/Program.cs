using System;
using System.Security.Cryptography;
using System.Text;

namespace Backrole.Crypto.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var Secp = Signs.Default.Get("SECP256K1");
            var MyKey = Secp.MakeKeyPair(true);

            var Data = Encoding.UTF8.GetBytes("Hello World!");
            var Sign = MyKey.SignSeal(Data);

            var Hash = Hashes.Default.Hash("DSHA256", Sign.Value);

            Console.WriteLine($"Key : {MyKey}\n - {MyKey.Value.ToBase58(true)}");
            Console.WriteLine($"Sign: {Sign}\n - {Sign.Value.ToBase58(true)}");
            Console.WriteLine($"Hash: {Hash}\n - {Hash.Value.ToBase58(true)}");

            Hash.Value.ToBase58();

            if (Sign.Verify(Data))
            {
                Console.WriteLine($"the signature verified successfully.");
            }

            if (Hash.Verify(Sign.Value))
            {
                Console.WriteLine($"the hash verified successfully.");
            }

            if (SignKeyPair.Parse(MyKey.ToString()) == MyKey)
            {
                Console.WriteLine($"Key parsed successfully.");
            }
            
            if (SignSealValue.Parse(Sign.ToString()) == Sign)
            {
                Console.WriteLine($"Sign parsed successfully.");
            }

            foreach (var HashName in Hashes.Default.Supports)
            {
                var Algorithm = Hashes.Default.Get(HashName);
                Hash = Algorithm.Hash(Data);

                Console.WriteLine($"Hash: {Hash} ({HashName})\n - {Hash.Value.ToBase58(true)}");
            }

            foreach (var SignName in Signs.Default.Supports)
            {
                var Algorithm = Signs.Default.Get(SignName);
                MyKey = Algorithm.MakeKeyPair(true);
                Sign = MyKey.SignSeal(Data);

                Console.WriteLine($"---\n{SignName}");
                Console.WriteLine($"Key : {MyKey}");
                Console.WriteLine($"Sign: {Sign}");

                if (Sign.Verify(Data))
                {
                    Console.WriteLine($"the signature verified successfully.");
                }
            }
        }
    }
}
