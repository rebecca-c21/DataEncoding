using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace DataEncoding
{
    public class FileDataEncoder
    {

        #region Properties

        /// <summary>
        /// An Encryption Algorithm.  Doesn't matter what kind!
        /// </summary>
        private IEncryptionAlgorithm EncryptionAlgorithm;

        /// <summary>
        /// The Raw Data read from this File
        /// </summary>
        public byte[] RawBytes
        {
            get;
            /// protected set means that this data cannot be changed from outside of this class
            protected set;
        }

        /// <summary>
        /// Get this data represented in a Base64 string suitable for transmission over the intarwebz
        /// by the way this construction is a readonly function.  You cannot edit this property.
        /// </summary>
        public string Base64String => Convert.ToBase64String(this.RawBytes);

        #endregion // Properties

        #region Constrcutors

        /// <summary>
        /// Get Data From a File given the full path to that file on your computer.
        /// </summary>
        /// <param name="filePath">a complete file path to the file you want to read.</param>
        public FileDataEncoder(string filePath)
        {
            /// this line may throw various File access related Exceptions
            /// FileNotFound if the file does not exist
            /// If the file is open in another application and locked (for example a spreadsheet in Excel?) it will also throw an Exception.
            /// Hover your mouse over the "FileStream" after the 'new' keyword to see a full list of the Exceptions this method may throw.
            /// 
            /// the FileStream object allows us to interact with the File directly in the code.
            using (FileStream file = new FileStream(filePath, FileMode.Open))
            {
                /// file.Length is the number of bytes available in the file.
                /// make a spot in memory the correct size to hold all the bytes of this file.
                this.RawBytes = new byte[file.Length];

                /// read each byte from the file.
                for (long i = 0; i < file.Length; i++)
                {
                    /// for reasons I could not begin to explain, the "ReadByte" method explicitly returns an int (32-bit value) that only uses 8-bits.
                    /// the (byte) in front converts to the correct data type.
                    this.RawBytes[i] = (byte)file.ReadByte();
                }
            }
        }

        /// <summary>
        /// Pretty simple and straight forward way to throw data into this object for manipulation.
        /// </summary>
        /// <param name="bytes">Raw Data</param>
        public FileDataEncoder(byte[] bytes)
        {
            this.RawBytes = bytes;
        }

        /// <summary>
        /// A slightly more generic version of the fileName constructor.
        /// This version takes any old stream, even one that is the Content from an HttpResponseMessage ;)
        /// 
        /// Note that NOT ALL Streams support the Length operator, so I can't use the same method as the FileStream case.
        /// </summary>
        /// <param name="stream">Any old data stream, no matter where it's from!  The internet, your computer, a random bit generator...</param>
        /// <param name="autoClose">do you want to automatically close the stream after reading?  Typically the answer is no.</param>
        public FileDataEncoder(Stream stream, bool autoClose = false)
        {
            /// because I'm lazy I'm using the List data type here...
            List<byte> buffer = new List<byte>();

            /// Keep reading until you can't read anymore!
            /// ... one byte at a time.
            while(stream.CanRead)
            {
                buffer.Add((byte)stream.ReadByte());
            }

            /// convert this to a standard array (because it has a smaller memory overhead)
            this.RawBytes = buffer.ToArray();

            if (autoClose) stream.Close();
        }

        #endregion // Constructors

        #region Methods

        /// <summary>
        /// Get the raw text representation of this Data.
        /// If this data is not actually Plain Text this is going to return garbledy-gook
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Encoding.UTF8.GetString(this.RawBytes);

        /// <summary>
        /// You could also use a different encoding if you like--but again, it is definitely 
        /// going to be garbledy-gook if the data here isn't plain text.
        /// </summary>
        /// <param name="encoding">What encoding do you want to use?</param>
        /// <returns></returns>
        public string ToString(Encoding encoding) => encoding.GetString(this.RawBytes);

        /// <summary>
        /// Get this Data formatted in Old School Hex format each line.
        /// By default, each line is two DWORDs (also known as 4 bytes).
        /// This is output to look like old school hacking games ;)
        /// </summary>
        /// <param name="columns">number of DWORDs to display per line.</param>
        /// <returns></returns>
        public string ToHexString(int columns = 2)
        {
            string output = "0\t";
            int colCount = 0;
            int lineCount = 0;
            foreach(byte b in RawBytes)
            {
                output += string.Format("{0:X}{1:X}{2}", b / 0x10, b % 0x10, ++colCount % 4 == 0 ? "  " : " ");

                if (colCount == 4 * columns) output += string.Format("{0}{1}\t", Environment.NewLine, ++lineCount * 4 * columns);
            }

            return output;
        }

        /// <summary>
        /// Save the data to a file
        /// </summary>
        /// <param name="filePath">full path to the location</param>
        /// <param name="overwrite">automatically delete the file if it already exists</param>
        public void Save(string filePath, bool overwrite = true)
        {
            if (overwrite && File.Exists(filePath)) File.Delete(filePath);

            FileStream file = new FileStream(filePath, FileMode.Create);
            file.Write(this.RawBytes, 0, this.RawBytes.Length);
            file.Flush();
            file.Close();
        }

        /// <summary>
        /// save to a referenced stream
        /// </summary>
        /// <param name="stream">Write this data directly to a given stream (like an HttpReqeustMessage or a Socket connection).</param>
        public void Save(ref Stream stream) => stream.Write(this.RawBytes, 0, this.RawBytes.Length);


        /// <summary>
        /// Encrypt the contents of this block using the provided encryption algorithm
        /// </summary>
        public void Encrypt()
        {
            for(long i = 0; i < RawBytes.Length; i+=EncryptionAlgorithm.BlockSize)
            {
                // Prepare a temporary space for this step in Encryption
                byte[] temp = new byte[EncryptionAlgorithm.BlockSize];

                // Move data into this temporary location
                for(int j = 0; j < EncryptionAlgorithm.BlockSize; j++)
                {
                    if (i + j < RawBytes.Length)
                        temp[j] = RawBytes[i + j];
                    
                    else  
                        temp[j] = 0;
                    // Have to be careful because it is possible to run over
                    // the end of the RawBytes size.  If this is the case, pad the temp array with zeros.
                }

                /// Encrypt this block of data.
                temp = EncryptionAlgorithm.EncryptBlock(temp);

                // Now restore the encrypted bytes into the RawBytes list.
                for (int j = 0; j < EncryptionAlgorithm.BlockSize; j++)
                {
                    if (i + j < RawBytes.Length)
                        RawBytes[i + j] = temp[j];
                    else
                        break;
                    // If there were overflow bits, we will just ignore them.
                }
            }

            // This file Data is now encrypted!
        }

        /// <summary>
        /// Encrypt the contents of this block using the provided encryption algorithm
        /// </summary>
        public void Decrypt()
        {
            for (long i = 0; i < RawBytes.Length; i += EncryptionAlgorithm.BlockSize)
            {
                // Prepare a temporary space for this step in Encryption
                byte[] temp = new byte[EncryptionAlgorithm.BlockSize];

                // Move data into this temporary location
                for (int j = 0; j < EncryptionAlgorithm.BlockSize; j++)
                {
                    if (i + j < RawBytes.Length)
                        temp[j] = RawBytes[i + j];

                    else
                        temp[j] = 0;
                    // Have to be careful because it is possible to run over
                    // the end of the RawBytes size.  If this is the case, pad the temp array with zeros.
                }

                /// Encrypt this block of data.
                temp = EncryptionAlgorithm.DecryptBlock(temp);

                // Now restore the encrypted bytes into the RawBytes list.
                for (int j = 0; j < EncryptionAlgorithm.BlockSize; j++)
                {
                    if (i + j < RawBytes.Length)
                        RawBytes[i + j] = temp[j];
                    else
                        break;
                    // If there were overflow bits, we will just ignore them.
                }
            }

            // This file Data is now encrypted!
        }

        #endregion // Methods

        #region Static Methods

        /// <summary>
        /// Get an Encoder object from Base64String data.
        /// </summary>
        /// <param name="base64string"></param>
        /// <returns></returns>
        public static FileDataEncoder FromBase64String(string base64string)
        {
            return new FileDataEncoder(Convert.FromBase64String(base64string));
        }

        #endregion
    }
}
