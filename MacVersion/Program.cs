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
            Console.WriteLine("Enter a file name (full path):");
            string filePath = Console.ReadLine();

            FileDataEncoder encoder = new FileDataEncoder(filePath);
            string b64s = encoder.Base64String;

            Console.WriteLine("Enter a file name to save base64 text to:");
            string outfilePath = Console.ReadLine();

            using (StreamWriter writer = new StreamWriter(new FileStream(outfilePath, FileMode.Create)))
            {
                writer.Write(b64s);
                writer.Flush();
            }

            Console.ReadLine();
        }
    }
}
