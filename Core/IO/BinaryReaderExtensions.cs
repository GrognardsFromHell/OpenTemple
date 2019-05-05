using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;

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

        /// <summary>
        /// Reads a 8-byte game location from the stream.
        /// </summary>
        /// <param name="reader"></param>
        public static LocAndOffsets ReadLocationAndOffsets(this BinaryReader reader)
        {
            var loc = new LocAndOffsets();
            loc.location.locx = reader.ReadInt32();
            loc.location.locy = reader.ReadInt32();
            loc.off_x = reader.ReadSingle();
            loc.off_y = reader.ReadSingle();
            return loc;
        }

        /// <summary>
        /// Reads a 8-byte game location from the stream.
        /// </summary>
        /// <param name="reader"></param>
        public static locXY ReadTileLocation(this BinaryReader reader)
        {
            // TODO: Verify that this is actually correct
            return new locXY(
                reader.ReadInt32(),
                reader.ReadInt32()
            );
        }

        /// <summary>
        /// Reads a ToEE object id from the reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ObjectId ReadObjectId(this BinaryReader reader)
        {
            Span<byte> buffer = stackalloc byte[24];
            if (reader.Read(buffer) != buffer.Length)
            {
                throw new Exception("Failed to read 24-byte ObjectId");
            }

            var type = BitConverter.ToUInt16(buffer.Slice(0, sizeof(ushort)));

            var payload = buffer.Slice(8); // ObjectId seems to use 8-byte packed

            switch (type)
            {
                case 0:
                    return ObjectId.CreateNull();
                case 1:
                    var protoId = BitConverter.ToUInt16(payload.Slice(0, sizeof(ushort)));
                    return ObjectId.CreatePrototype(protoId);
                case 2:
                    var guid = MemoryMarshal.Read<Guid>(payload);
                    return ObjectId.CreatePermanent(guid);
                case 3:
                    var positionalId = MemoryMarshal.Read<PositionalId>(payload);
                    return ObjectId.CreatePositional(positionalId);
                case 0xFFFE:
                    var handle = MemoryMarshal.Read<ObjHndl>(payload);
                    return ObjectId.CreateHandle(handle);
                case 0xFFFF:
                    return ObjectId.CreateBlocked();
                default:
                    throw new Exception("Unknown ObjectId type: " + type);
            }
        }

        /// <summary>
        /// Return true if the reader is at the end of file based on the underlying stream's position.
        /// </summary>
        public static bool AtEnd(this BinaryReader reader)
        {
            return reader.BaseStream.Position >= reader.BaseStream.Length;
        }

    }
}