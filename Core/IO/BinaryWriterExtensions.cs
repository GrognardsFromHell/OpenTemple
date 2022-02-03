using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.IO;

[SuppressMessage("ReSharper", "RedundantCast")]
public static class BinaryWriterExtensions
{
    /// <summary>
    /// Writes a string prefixed with its length as a 32-bit integer using the default platform encoding.
    /// The string is not null terminated.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="str"></param>
    public static void WritePrefixedString(this BinaryWriter writer, string str)
    {
        var bytes = Encoding.Default.GetBytes(str);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    /// <summary>
    /// Writes a fixed size string that is null-terminated.
    /// </summary>
    public static void WriteFixedString(this BinaryWriter writer, int length, ReadOnlySpan<char> text)
    {
        Span<byte> buffer = stackalloc byte[length];
        var byteCount = Encoding.Default.GetBytes(text, buffer);
        buffer[byteCount] = 0;
        writer.Write(buffer);
    }

    /// <summary>
    /// Write a 16-byte game location including offsets to the stream.
    /// </summary>
    public static void WriteLocationAndOffsets(this BinaryWriter writer, in LocAndOffsets locAndOffsets)
    {
        writer.Write(locAndOffsets.location.locx);
        writer.Write(locAndOffsets.location.locy);
        writer.Write(locAndOffsets.off_x);
        writer.Write(locAndOffsets.off_y);
    }

    public static void WriteTileLocation(this BinaryWriter writer, in locXY location)
    {
        writer.Write(location.locx);
        writer.Write(location.locy);
    }

    /// <summary>
    /// Writes a ToEE object id.
    /// </summary>
    public static void WriteObjectId(this BinaryWriter writer, in ObjectId id)
    {
        Span<byte> buffer = stackalloc byte[24];
        var payload = buffer.Slice(8); // ObjectId seems to use 8-byte packed

        var success = true;

        switch (id.Type)
        {
            case ObjectIdKind.Null:
                success &= BitConverter.TryWriteBytes(buffer, (ushort) 0);
                break;
            case ObjectIdKind.Prototype:
                success &= BitConverter.TryWriteBytes(buffer, (ushort) 1);
                ushort protoId = id.PrototypeId;
                success &= BitConverter.TryWriteBytes(payload, protoId);
                break;
            case ObjectIdKind.Permanent:
                success &= BitConverter.TryWriteBytes(buffer, (ushort) 2);
                var guid = id.PermanentId;
                MemoryMarshal.Write(payload, ref guid);
                break;
            case ObjectIdKind.Positional:
                success &= BitConverter.TryWriteBytes(buffer, (ushort) 3);
                var positional = id.PositionalId;
                MemoryMarshal.Write(payload, ref positional);
                break;
            case ObjectIdKind.Handle:
                throw new ArgumentException("Cannot write an objectid containing a live object!");
            case ObjectIdKind.Blocked:
                success &= BitConverter.TryWriteBytes(buffer, (ushort) 0xFFFF);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!success)
        {
            throw new Exception("Failed to serialize ObjectId");
        }

        writer.Write(buffer);
    }

    /// <summary>
    /// Frozen object references include the object's GUID, but also the current map and location.
    /// This is mostly pointless since the GUID should be sufficient.
    /// </summary>
    public static void WriteFrozenObjRef(this BinaryWriter writer, FrozenObjRef objRef)
    {
        writer.WriteObjectId(objRef.guid);
        writer.WriteTileLocation(objRef.location);
        writer.WriteInt32( objRef.mapNumber);
    }

    public static void WriteGameTime(this BinaryWriter writer, GameTime time)
    {
        writer.WriteInt32( time.timeInDays);
        writer.WriteInt32( time.timeInMs);
    }

    private const int SecondsPerDay = 24 * 60 * 60;

    public static void WriteGameTime(this BinaryWriter writer, TimeSpan time)
    {
        var ms = (long) time.TotalMilliseconds;
        var msecs = ms % (SecondsPerDay * 1000);
        var days = ms / (SecondsPerDay * 1000);

        writer.WriteInt32((int) days);
        writer.WriteInt32((int) msecs);
    }

    public static void WriteGameTime(this BinaryWriter writer, TimePoint time)
        => WriteGameTime(writer, time.ToGameTime());

    // Using Write is too dangerous if the type of the passed in value is ever changed, we'll not notice
    // the resulting data corruption (since a 16-bit value may become 32-bit, etc.)
    // So instead we use these explicit length write functions.
    public static void WriteInt32(this BinaryWriter writer, int value) => writer.Write(value);

    public static void WriteUInt32(this BinaryWriter writer, uint value) => writer.Write(value);

    public static void WriteInt64(this BinaryWriter writer, long value) => writer.Write(value);

    public static void WriteUInt64(this BinaryWriter writer, ulong value) => writer.Write(value);

    public static void WriteInt16(this BinaryWriter writer, short value) => writer.Write(value);

    public static void WriteUInt16(this BinaryWriter writer, ushort value) => writer.Write(value);

    public static void WriteInt8(this BinaryWriter writer, sbyte value) => writer.Write(value);

    public static void WriteUInt8(this BinaryWriter writer, byte value) => writer.Write(value);

    public static void WriteSingle(this BinaryWriter writer, float value) => writer.Write(value);

    public static unsafe void WriteIndexTable<T>(this BinaryWriter writer, ICollection<KeyValuePair<int, T>> items) where T : unmanaged
    {
        writer.WriteUInt32(0xAB1EE1BAu);

        writer.WriteInt32(1); // Bucket count
        writer.WriteInt32(sizeof(T));

        Span<T> valueBuffer = stackalloc T[1];
        var valueByteView = MemoryMarshal.Cast<T, byte>(valueBuffer);

        writer.WriteInt32(items.Count); // Node count
        foreach (var (key, value) in items)
        {
            writer.WriteInt32(key);

            valueBuffer[0] = value;
            writer.Write(valueByteView);
        }

        writer.WriteUInt32(0xE1BAAB1Eu);
    }

    public delegate void IndexTableItemWriter<in T>(BinaryWriter writer, T item);

    public static void WriteIndexTable<T>(this BinaryWriter writer,
        ICollection<KeyValuePair<int, T>> items,
        int itemSize, IndexTableItemWriter<T> itemWriter)
    {
        writer.WriteUInt32(0xAB1EE1BAu);

        writer.WriteInt32(1); // Bucket count
        writer.WriteInt32(itemSize);

        writer.WriteInt32(items.Count); // Node count
        foreach (var (key, item) in items)
        {
            writer.WriteInt32(key);
            itemWriter(writer, item);
        }

        writer.WriteUInt32(0xE1BAAB1Eu);
    }

}