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
            Console.WriteLine("/Users/Rebecca/Desktop/vermin_b64.txt");
            string filePath = Console.ReadLine();

            FileDataEncoder encoder = new FileDataEncoder(filePath);
            string b64s = encoder.Base64String;

            Console.WriteLine("/User/Rebecca/Desktop/pee.png");
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
