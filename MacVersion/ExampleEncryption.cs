#define SHOW_WORK

using System;
namespace DataEncoding
{
    public class ExampleEncryption : IEncryptionAlgorithm
    {
        /// <summary>
        /// This encryption algorithm has a block size of 2
        /// </summary>
        public int BlockSize => 2;

        /// <summary>
        /// a /really large/ prime number (not the largest 64-bit prime, but close)
        /// </summary>
        public static ulong prime => (ulong)Math.Pow(2, 61) - 1;

        /// <summary>
        /// Another large prime, though this one is less important that it is prime.  
        /// It must be shared publicly in order to encrypt.
        /// </summary>
        public static ulong initialState => (ulong)Math.Pow(2, 31) - 1;

        /// <summary>
        /// seed for this algorithm.  Ideally, this is negotiated between two partners
        /// using a Diffie-Helman Key Exchange.
        /// For Added security, a different key for each message sent.
        /// </summary>
        private ulong seed;

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
        private ushort key
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
            state = initialState;
        }
       
        public ExampleEncryption(ulong seed)
        {
            this.seed = seed;
            this.state = initialState;
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

            byte[] keyBytes = { (byte)(key / 256), (byte)(key % 256) };

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

            UpdateState();

            return result;
        }

        /// <summary>
        /// In this particular algorithm, these two methods are identical!
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public byte[] DecryptBlock(byte[] block) => EncryptBlock(block);

        /// <summary>
        /// Compute new state as (state ^ seed) % prime
        /// This is really the pseudo random bit generator which is necessary to make a secure key.
        /// </summary>
        public void UpdateState()
        {
            // hold onto the current state as it is used in the calculation
            // We can't just say 'state = Math.Pow(state, seed);', because 
            // that number will likely be so big you blow up your computer.
            ulong temp = state;

            // These numbers are VERY large, so in order to make sure we 
            // don't overflow a 64-bit integer, we need to do this exponent
            // one multiplication at a time and % out the prime at each step.
            for(ulong i = 0; i < this.seed; i++)
            {
                temp = (temp * state) % prime;
            }

            // now store the new state.
            state = temp;
        }
    }
}
