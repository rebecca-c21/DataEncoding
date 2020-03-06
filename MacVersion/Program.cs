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
            FileFromBase64();

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        static void FileToBase64()
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
        }
        //we use try catches because if you cant open the file you need to tell the user why
        //you also want to try to see if there's any data at all

        static void FileFromBase64()
        {
            Console.WriteLine("Enter a file name containing base64 encoded text:");
            string filePath = Console.ReadLine();
            string base64string = "";
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    base64string = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if(!string.IsNullOrEmpty(base64string))
            {
                Console.WriteLine("Enter a file name for the output:");
                string outfilePath = Console.ReadLine();

                FileDataEncoder encoder = FileDataEncoder.FromBase64String(base64string);

                encoder.Save(outfilePath);
                Console.WriteLine(encoder.ToHexString());
            }
        }
    }
}
