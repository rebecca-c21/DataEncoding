#define SHOW_WORK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataEncoding
{
    /// <summary>
    /// An example encryption algorithm that uses a simplified version of RSA Encryption.
    /// A given prime (2^61-1) and starting point (2^31-1) are used to generate a 
    /// pseudo random list of bits used as the key for encryption.
    /// 
    /// The UpdateState() method is a rigorously defined one way function meaning
    /// that calculating the new state is trivially easy, however going backwards 
    /// to find what the input to the function was is incredibly difficult.
    /// </summary>
    public class ExampleEncryption : IEncryptionAlgorithm
    {
        /// <summary>
        /// This encryption algorithm has a block size of 2 bytes (16-bit).
        /// </summary>
        public int BlockSize => 2;

        /// <summary>
        /// a /really large/ prime number For this case, we are using a 32-bit prime because our multiplication algorithm SUX and is super slow.
        /// </summary>
        public static ulong prime
        {
            get
            {
                if(_prime == 0) 
                    _prime = (ulong)Math.Pow(2, 31) - 1;
                return _prime;
            }
        }
        private static ulong _prime;


        /// <summary>
        /// Another large prime, though this one is less important that it is prime.  
        /// It must be shared publicly in order to encrypt.
        /// </summary>
        public static ulong generator
        {
            get
            {
                if (_generator == 0) 
                    _generator = (ulong)Math.Pow(2, 17) - 1;
                return _generator;
            }
        }
        private static ulong _generator;

        /// <summary>
        /// seed for this algorithm.  Ideally, this is negotiated between two partners
        /// using a Diffie-Helman Key Exchange.
        /// For Added security, a different key for each message sent.
        /// </summary>
        private ulong privateKey;

        /// <summary>
        /// Public Key to be shared with another person!
        /// </summary>
        public ulong publicKey
        {
            get
            {
                if(_publicKey == 0)
                    _publicKey = BigPow(generator, privateKey);

                return _publicKey;
            }
        }
        private ulong _publicKey;

        /// <summary>
        /// the Key that is only acquireable by knowing your Private Key!
        /// </summary>
        private ulong combinedKey;

        /// <summary>
        /// current internal state
        /// </summary>
        private ulong state;

        /// <summary>
        /// current key
        /// This is the last 16-bits of the current 64-bit state.
        /// These are the two bytes that are XOR with a block of data to encrypt/decrypt it.
        /// The security comes in the fact that it is practically (for a mathematically provable definition of "practical")
        /// impossible to take this key and regenerate the state of this object or the seed used to get here.
        /// </summary>
        private ushort keyBytes
        {
            get
            {
                // take just the last 16-bits
                ulong temp = state % ushort.MaxValue;
                return (ushort)temp;
            }
        }

        /// <summary>
        /// Reset this object.
        /// </summary>
        public void Reset()
        {
            state = generator;
        }
       
        /// <summary>
        /// using a private and public key.
        /// </summary>
        /// <param name="priv">Your private key!  Not shared with /anyone!/</param>
        /// <param name="pub">The Public Key of the person you are communicating with.  The person you are communicating with will need to use your public key also.</param>
        public ExampleEncryption(ulong priv, ulong pub)
        {
            this.privateKey = priv;
#if SHOW_WORK
            Console.WriteLine("Calculating Combined Key...");
#endif
            this.combinedKey = BigPow(pub, priv);
            this.state = generator;
        }

        /// <summary>
        /// Calculates (b^exp) % prime
        /// </summary>
        /// <param name="b"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static ulong BigPow(ulong b, ulong exp)
        {
            ulong temp = b;

            for(ulong i = 0; i < exp; i++)
            {
                temp = (temp * b) % prime;
            }

            return temp;
        }

        private static IEnumerable<ulong> Range(ulong fromInclusive, ulong toExclusive)
        {
            for (ulong i = fromInclusive; i < toExclusive; i++) yield return i;
        }

        /// <summary>
        /// Encrypts a 16-bit block (e.g. 2 bytes) using a 64-bit encryption bit generator.
        /// After using the last 16-bits of the next step in the generator, calls UpdateState()
        /// to prepare for encrypting the next block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public byte[] EncryptBlock(byte[] block)
        {
            if (block.Length != 2) throw new ArgumentException("Block Size must be 2 bytes", "block");

#if SHOW_WORK
            Console.WriteLine("** Updating State **");
#endif

            UpdateState();

            byte[] keyBytes = { (byte)(this.keyBytes / 256), (byte)(this.keyBytes % 256) };

            /// ^ is not exponent!  it is the XOR operator!  WTF is that!?!? 
            byte[] result = { (byte)(block[0] ^ keyBytes[0]), (byte)(block[1] ^ keyBytes[1]) };

// this code only runs if you have uncommented the '#define SHOW_WORK' line at the top of this file.
// it prints a nice arithmetic statement that lets you see the XOR at work for each pair of bytes.
#if SHOW_WORK
            Console.WriteLine("\t{0} {1}", Convert.ToString((char)block[0]).PadLeft(8, ' '), Convert.ToString((char)block[1]).PadLeft(8, ' '));
            Console.WriteLine("\t{0} {1}", Convert.ToString(block[0], 2).PadLeft(8, '0'), Convert.ToString(block[1], 2).PadLeft(8, '0'));
            Console.WriteLine("^\t{0} {1}", Convert.ToString(keyBytes[0], 2).PadLeft(8, '0'), Convert.ToString(keyBytes[1], 2).PadLeft(8, '0'));
            Console.WriteLine("__________________________________________");
            Console.WriteLine("\t{0} {1}", Convert.ToString(result[0], 2).PadLeft(8, '0'), Convert.ToString(result[1], 2).PadLeft(8, '0'));

            Console.ReadLine();
#endif


            return result;
        }

        /// <summary>
        /// In this particular algorithm, these two methods are identical!
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public byte[] DecryptBlock(byte[] block) => EncryptBlock(block);

        /// <summary>
        /// Compute new state as (state ^ combinedKey) % prime
        /// This is really the pseudo random bit generator which is necessary to make a secure key.
        /// </summary>
        public void UpdateState() => state = BigPow(state, combinedKey);
    }
}
