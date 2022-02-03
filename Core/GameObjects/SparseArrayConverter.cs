using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenTemple.Core.GameObjects;

public static class SparseArrayConverter
{
    public delegate void ItemWriter<in T>(BinaryWriter writer, T items);

    public static void WriteTo<T>(BinaryWriter writer,
        int elementSize,
        IList<T> items,
        ItemWriter<T> itemWriter)
    {
        writer.Write(elementSize);
        writer.Write(items.Count);
        writer.Write(0); // Was previously the array index id, but this is transient data

        var startPos = writer.BaseStream.Position;
        foreach (var item in items)
        {
            itemWriter(writer, item);
        }
        Debug.Assert(writer.BaseStream.Position - startPos == elementSize * items.Count);

        WriteDenseIndexBitmap(writer, items.Count);
    }

    // ToEE used sparse arrays that had an index bitmap at the end, since we're writing a dense list
    // we'll have to write out a fake bitmap to be compatible with the old format.
    internal static void WriteDenseIndexBitmap(BinaryWriter writer, int elementCount)
    {
        // Save an index bitmap that is contiguous for the number of elements
        var blockElements = (elementCount + 31) / 32;
        writer.Write(blockElements);
        for (var i = 0; i < blockElements - 1; i++)
        {
            writer.Write(uint.MaxValue);
        }

        if (blockElements > 0)
        {
            var lastBlockEls = elementCount % 32;
            if (lastBlockEls == 0)
            {
                lastBlockEls = 32;
            }

            uint lastBlock = ~(uint.MaxValue << lastBlockEls);

            writer.Write(lastBlock);
        }
    }

    public delegate T ItemReader<out T>(BinaryReader reader);

    public static List<T> ReadFrom<T>(BinaryReader reader, int itemSize, ItemReader<T> itemReader,
        bool keepGaps)
    {
        var elementSize = reader.ReadInt32();
        var count = reader.ReadInt32();
        reader.ReadInt32(); // Unused transient data

        if (elementSize != itemSize)
        {
            throw new Exception($"Read element size {elementSize} from stream, but " +
                                $"expected {itemSize}");
        }

        var result = new List<T>(count);

        for (var i = 0; i < count; i++)
        {
            var startPos = reader.BaseStream.Position;
            result.Add(itemReader(reader));
            var bytesRead = reader.BaseStream.Position - startPos;
            if (bytesRead != itemSize)
            {
                throw new IOException($"Item writer read {bytesRead} bytes. Expected {itemSize}.");
            }
        }

        var blockCount = reader.ReadInt32();
        if (!keepGaps)
        {
            for (var i = 0; i < blockCount; i++)
            {
                reader.ReadInt32();
            }
        }
        else
        {
            var currentIdx = 0;
            for (var i = 0; i < blockCount; i++)
            {
                var indexBlock = reader.ReadUInt32();
                if (indexBlock == uint.MaxValue)
                {
                    // Easy case, fully populated
                    currentIdx += 32;
                    continue;
                }

                for (var j = 0; j < 32; j++)
                {
                    var mask = (1u << j);
                    if ((indexBlock & mask) == 0)
                    {
                        // Insert a default value
                        if (currentIdx < result.Count)
                        {
                            result.Insert(currentIdx, default);
                        }
                        else
                        {
                            result.Add(default);
                        }
                    }

                    currentIdx++;
                }
            }
        }

        return result;
    }
}