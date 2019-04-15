using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace SpicyTemple.Core.IO
{
    static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a string from the stream that is prefixed with a 32-bit integer containing its length. The
        /// string is assumed to not be null terminated. The string is decoded using the default encoding for
        /// this platform.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The string read from the reader.</returns>
        public static string ReadPrefixedString(this BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var data = reader.ReadBytes(length);

            // Decode using local encoding
            return Encoding.Default.GetString(data);
        }

        /// <summary>
        /// Reads a fixed size string that is null-terminated.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The string read from the reader.</returns>
        public static string ReadFixedString(this BinaryReader reader, int length)
        {
            Span<byte> buffer = stackalloc byte[length];
            reader.Read(buffer);

            var actualLength = 0;
            for (; actualLength < buffer.Length && buffer[actualLength] != 0; actualLength++)
            {
            }

            // Decode using local encoding
            return Encoding.Default.GetString(buffer.Slice(0, actualLength));
        }

        /// <summary>
        /// Reads a vector3 from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The string read from the reader.</returns>
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Reads a vector4 from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The string read from the reader.</returns>
        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            var w = reader.ReadSingle();
            return new Vector4(x, y, z, w);
        }

        /*
        /// <summary>
        /// Reads a 8-byte game location from the stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="loc"></param>
        public static Location ReadLocation(this BinaryReader reader)
        {
            var loc = new Location();
            loc.X = reader.ReadInt32();
            loc.Y = reader.ReadInt32();
            loc.OffsetX = reader.ReadSingle();
            loc.OffsetY = reader.ReadSingle();
            return loc;
        }

        /// <summary>
        /// Reads a ToEE object id from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ObjectGuid ReadObjectGuid(this BinaryReader reader)
        {
            var result = new ObjectGuid();

            result.Type = reader.ReadInt16();
            result.Foo = reader.ReadInt16();
            result.Foo2 = reader.ReadInt32();
            var guidData = reader.ReadBytes(16);
            result.GUID = new Guid(guidData);

            return result;
        }*/

    }
        
}
