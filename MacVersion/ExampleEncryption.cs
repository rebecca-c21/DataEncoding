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
        /// a /really large/ prime number
        /// </summary>
        public static ulong prime => (ulong)Math.Pow(2, 61) - 1;

        /// <summary>
        /// Another large prime, though this one is less important that it is prime.  It must be
        /// </summary>
        public static ulong initialState => (ulong)Math.Pow(2, 31) - 1;

        /// <summary>
        /// seed for this algorithm
        /// </summary>
        private ulong seed;

        /// <summary>
        /// current internal state
        /// </summary>
        private ulong state;

        /// <summary>
        /// current key
        /// </summary>
        private ushort key
        {
            get
            {
                ulong temp = state % ushort.MaxValue;
                return (ushort)temp;
            }
        }

       
        public ExampleEncryption(ulong seed)
        {
            this.seed = seed;
            this.state = 
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

            uint temp = (uint)256 * block[0] + block[1];
            temp = (temp + key) % ushort.MaxValue;

            byte[] result = { (byte)(temp / 256), (byte)(temp % 256) };

            UpdateState();

            return result;
        }

        public byte[] DecryptBlock(byte[] block)
        {
            if (block.Length != 2) throw new ArgumentException("Block Size must be 2 bytes", "block");

            uint temp = (uint)256 * block[0] + block[1];
            temp = (temp + key) % ushort.MaxValue;

            byte[] result = { (byte)(temp / 256), (byte)(temp % 256) };

            UpdateState();

            return result;
        }

        /// <summary>
        /// Compute new state as (state ^ seed) % prime
        /// </summary>
        public void UpdateState()
        {
            ulong temp = state;

            for(ulong i = 0; i < this.seed; i++)
            {
                temp = (temp * state) % prime;
            }

            state = temp;
        }
    }
}
