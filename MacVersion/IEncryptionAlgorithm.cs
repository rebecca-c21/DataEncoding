using System;
namespace DataEncoding
{
    /// <summary>
    /// Interface for an EncryptionAlgorithm
    /// </summary>
    public interface IEncryptionAlgorithm
    {
        /// <summary>
        /// Encrypt a given block of data
        /// </summary>
        /// <param name="block">array of bytes to be encrypted</param>
        /// <returns></returns>
        byte[] EncryptBlock(byte[] block);

        byte[] DecryptBlock(byte[] block);

        /// <summary>
        /// Update the state of the random bit generator
        /// </summary>
        /// <returns>unsigned long (64-bit non negative integer)</returns>
        void UpdateState();

        void Reset();

        /// <summary>
        /// Must define a block size in bytes
        /// </summary>
         int BlockSize { get; }
    }
}
