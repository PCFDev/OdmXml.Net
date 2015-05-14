using System;
using System.Collections.Generic;

namespace PCF.OdmXml.i2b2Importer.Helpers
{
    /// <summary>
    /// Represents an array of bytes.
    /// </summary>
    public class ByteArrayBulder
    {
        private List<byte> Bytes = new List<byte>();

        /// <summary>
        /// Construct an empty ByteArrayBuilder.
        /// </summary>
        public ByteArrayBulder()
        {
        }

        /// <summary>
        /// Construct a ByteArrayBuilder with an initial set of bytes.
        /// </summary>
        /// <param name="bytes"></param>
        public ByteArrayBulder(params byte[] bytes)
        {
            if (bytes != null)
                Bytes.AddRange(bytes);
        }

        /// <summary>
        /// Append one or more bytes to the byte array.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public ByteArrayBulder Append(params byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            Bytes.AddRange(bytes);
            return this;
        }

        /// <summary>
        /// Clears the byte array.
        /// </summary>
        public void Clear()
        {
            Bytes.Clear();
        }

        /// <summary>
        /// Get the built byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return Bytes.ToArray();
        }
    }
}
