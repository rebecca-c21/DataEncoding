using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataEncoding
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Gimme some text!");
            string text = Console.ReadLine();

            FileDataEncoder encoder = new FileDataEncoder(Encoding.ASCII.GetBytes(text)) { EncryptionAlgorithm = new ExampleEncryption((ulong)Math.Pow(2, 8) - 31) };

            encoder.Encrypt();
            Console.WriteLine(encoder);

            encoder.EncryptionAlgorithm.Reset();

            encoder.Decrypt();
            Console.WriteLine(encoder);

            Console.ReadLine();
        }
    }
}
