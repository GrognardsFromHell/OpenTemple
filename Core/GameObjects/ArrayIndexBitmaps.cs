using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenTemple.Core.GameObjects;

/// <summary>
/// Manages storage for array index bitmaps, which are used to more efficiently store sparse arrays.
/// </summary>
internal class ArrayIndexBitmaps
{

    public static ArrayIndexBitmaps Instance { get; } = new();

    public ArrayIndexBitmaps()
    {
        mBitmapBlocks = new List<uint>(8192);
        mArrays = new List<ArrayIndices>(4096);
        mFreeIds = new List<ArrayIdxMapId>(4096);

        // Initialize the bitmasks
        uint bitmask = 0; // No bits set
        mPartialBitmasks[0] = bitmask;

        for (var i = 1; i < 32; ++i)
        {
            // Set one more bit
            bitmask = (bitmask << 1) | 1;
            mPartialBitmasks[i] = bitmask;
        }

        // Initialize the bitcount lookup tables
        // Do it for 0-xFF first, then use that to do the upper word as well
        for (uint i = 0; i <= byte.MaxValue; ++i)
        {
            // Count bits set in i
            mBitCountLut[i] = PopCntSlow(i);
        }

        // Now initialize the rest
        for (uint i = byte.MaxValue + 1; i <= ushort.MaxValue; ++i)
        {
            // Count bits set in i
            var upper = (i >> 8) & 0xFF;
            var lower = i & 0xFF;
            mBitCountLut[i] = (byte) (mBitCountLut[upper] + mBitCountLut[lower]);
        }
    }

    private static byte PopCntSlow(uint value)
    {
        byte count = 0;
        while (value != 0)
        {
            if ((value & 1) != 0)
            {
                ++count;
            }

            value >>= 1;
        }

        return count;
    }

    // Allocate an array index map
    public ArrayIdxMapId Allocate()
    {
        // Take one from the pool if available
        if (mFreeIds.Count > 0)
        {
            var result = mFreeIds[mFreeIds.Count - 1];
            mFreeIds.RemoveAt(mFreeIds.Count - 1);
            return result;
        }

        // Otherwise add a new one
        var id = new ArrayIdxMapId(mArrays.Count);

        // Start with 2 blocks initially, which is enough to track indices 0-63
        ArrayIndices indices;
        indices.Count = 2;
        indices.FirstIdx = mBitmapBlocks.Count;
        mBitmapBlocks.Add(0);
        mBitmapBlocks.Add(0);
        mArrays.Add(indices);

        return id;
    }

    // Free an array index map
    public void Free(ArrayIdxMapId id)
    {
        var arr = mArrays[id.Id];

        // Shrink the array to 2 elements before returning it to the free list
        if (arr.Count > 2)
        {
            Shrink(id, arr.Count - 2);
            arr = mArrays[id.Id]; // Refresh the local struct
        }

        Trace.Assert(arr.Count == 2);

        // Reset the bitmaps
        for (int i = 0; i < arr.Count; ++i)
        {
            mBitmapBlocks[arr.FirstIdx + i] = 0;
        }

        mFreeIds.Add(id);
    }

    // Copies an array index map and returns the id of the created copy
    public ArrayIdxMapId Clone(ArrayIdxMapId id)
    {
        var result = Allocate();
        var dest = mArrays[result.Id];
        var src = mArrays[id.Id];

        // Make the destination big enough
        if (src.Count > dest.Count)
        {
            var extendBy = src.Count - dest.Count;
            Extend(result, extendBy);
        }

        // Copy over the bitmap blocks
        for (var i = 0; i < src.Count; i++)
        {
            mBitmapBlocks[dest.FirstIdx + i] = mBitmapBlocks[src.FirstIdx + i];
        }

        return result;
    }

    // Removes an index from an index map
    public void RemoveIndex(ArrayIdxMapId id, int index)
    {
        var arr = mArrays[id.Id];
        var blockIdx = index / 32;

        // The index is out of bounds
        if (blockIdx >= arr.Count)
        {
            // But since we're removing, it doesn't matter
            return;
        }

        // Retrieve a reference to the bitmap block that contains the
        // bit for the index
        var bitmapBlock = mBitmapBlocks[arr.FirstIdx + blockIdx];

        // Clear the bit that represents the index
        var bit = unchecked((uint) (1 << (index % 32)));
        mBitmapBlocks[arr.FirstIdx + blockIdx] = bitmapBlock & ~bit;
    }

    // Adds an index to an index map if it's not already in it
    public void AddIndex(ArrayIdxMapId id, int index)
    {
        var arr = mArrays[id.Id];
        var blockIdx = index / 32;

        // The index is out of bounds
        if (blockIdx >= arr.Count)
        {
            // We have to extend it to be able to store the index bit
            var extendBy = blockIdx + 1 - arr.Count;
            Extend(id, extendBy);
        }

        // Retrieve a reference to the bitmap block that contains the
        // bit for the index
        var bitmapBlock = mBitmapBlocks[arr.FirstIdx + blockIdx];

        // Set the bit that represents the index
        var bit = unchecked((uint) (1 << (index % 32)));
        mBitmapBlocks[arr.FirstIdx + blockIdx] = bitmapBlock | bit;
    }

    // Checks if an index is present in the array index map
    public bool HasIndex(ArrayIdxMapId id, int index)
    {
        var arr = mArrays[id.Id];
        var blockIdx = index / 32;
        var bitIdx = index % 32;

        if (blockIdx >= arr.Count)
        {
            return false; // No allocated bitmap block for this idx
        }

        var bitmapBlock = mBitmapBlocks[arr.FirstIdx + blockIdx];
        var mask = 1 << bitIdx;

        return (bitmapBlock & mask) != 0;
    }

    // Get the index mapped to a range with no gaps (for storage purposes)
    public int GetPackedIndex(ArrayIdxMapId id, int index)
    {
        var arr = mArrays[id.Id];
        var blockIdx = index / 32;

        int count = 0;

        // The number of fully counted blocks
        var fullCounted = Math.Min(arr.Count, blockIdx);
        for (int i = 0; i < fullCounted; ++i)
        {
            count += PopCnt(mBitmapBlocks[arr.FirstIdx + i]);
        }

        // The block that contains the actual index bit is counted
        // and and not including the index bit itself
        if (blockIdx < arr.Count)
        {
            byte bitIdx = (byte) (index % 32);
            count += PopCntConstrained(mBitmapBlocks[arr.FirstIdx + blockIdx], bitIdx);
        }

        return count;
    }

    // Serializes an array index map to the given stream
    public void SerializeToStream(ArrayIdxMapId id, BinaryWriter writer)
    {
        var arr = mArrays[id.Id];

        writer.Write(arr.Count);
        for (var i = 0; i < arr.Count; i++)
        {
            writer.Write(mBitmapBlocks[arr.FirstIdx + i]);
        }
    }

    // Deserializes an array index map from the given file and returns
    // the id of the newly allocated map or throws on failure.
    public ArrayIdxMapId DeserializeFromFile(BinaryReader reader)
    {
        var count = reader.ReadInt32();

        var result = Allocate();
        var arr = mArrays[result.Id];

        if (count > arr.Count)
        {
            var extendBy = count - arr.Count;
            Extend(result, extendBy);
        }

        for (int i = 0; i < count; i++)
        {
            mBitmapBlocks[arr.FirstIdx + i] = reader.ReadUInt32();
        }

        return result;
    }

    // Calls a callback for all indices that are present in the index map.
    // Stops iterating when false is returned. Returns false if any call to
    // callback returned false, true otherwise.
    public bool ForEachIndex(ArrayIdxMapId id, Func<int, bool> callback)
    {
        var arr = mArrays[id.Id];

        var index = 0;
        for (var i = 0; i < arr.Count; ++i)
        {
            var block = mBitmapBlocks[arr.FirstIdx + i];

            for (byte bitIdx = 0; bitIdx < 32; ++bitIdx)
            {
                // Index is present in the map
                if ((block & (1 << bitIdx)) != 0)
                {
                    if (!callback(index))
                    {
                        return false;
                    }
                }

                index++;
            }
        }

        return true;
    }

    // Count of set bits in given 32-bit integer up to and not including the given bit (0-31)
    public byte PopCntConstrained(uint value, byte upToExclusive)
    {
        // Use precomputed masks to unset the bits we don't want to count
        return PopCnt(value & mPartialBitmasks[upToExclusive]);
    }

    // Count of set bits in given 32-bit integer
    public byte PopCnt(uint value)
    {
        int lower = (int) (value & ushort.MaxValue);
        int upper = (int) ((value >> 16) & ushort.MaxValue);
        return (byte) (mBitCountLut[lower] + mBitCountLut[upper]);
    }

    // The bitmap blocks that contain the actual index bitmaps
    private List<uint> mBitmapBlocks;

    // State stored for each array
    private List<ArrayIndices> mArrays;

    // IDs of free entries in mArrays
    private List<ArrayIdxMapId> mFreeIds;

    // Bitmask lookup table that contains bitmasks that have the lower 0 - 31
    // bits set. This is used in counting the bits up and until position i in
    // a DWORD
    private uint[] mPartialBitmasks = new uint[32];

    // A bit count lookup table for all values of a 16-bit integer
    private byte[] mBitCountLut = new byte[ushort.MaxValue + 1];

    // Shrinks an index map by the specified number of bitmap blocks
    private void Shrink(ArrayIdxMapId id, int shrinkBy)
    {
        var arr = mArrays[id.Id];

        Trace.Assert(shrinkBy <= arr.Count);
        arr.Count -= shrinkBy;
        mArrays[id.Id] = arr;

        // Iterator to the first element to be removed
        var first = arr.FirstIdx + arr.Count;
        mBitmapBlocks.RemoveRange(first, shrinkBy);

        // Now the "firstIdx" of all arrays after the one we modified have to be adjusted
        for (int i = id.Id + 1; i < mArrays.Count; ++i)
        {
            var otherArr = mArrays[i];
            otherArr.FirstIdx -= shrinkBy;
            mArrays[i] = otherArr;
        }
    }

    // Extends an index map by the specified number of bitmap blocks
    private void Extend(ArrayIdxMapId id, int extendBy)
    {
        var arr = mArrays[id.Id];

        // Index to the position before which the new elements will be inserted
        var insertBefore = arr.FirstIdx + arr.Count;
        mBitmapBlocks.InsertRange(insertBefore, Enumerable.Repeat<uint>(0, extendBy));

        arr.Count += extendBy;
        mArrays[id.Id] = arr;

        // Now the "firstIdx" of all arrays after the one we modified have to be adjusted
        for (var i = id.Id + 1; i < mArrays.Count; ++i)
        {
            var otherArr = mArrays[i];
            otherArr.FirstIdx += extendBy;
            mArrays[i] = otherArr;
        }
    }

    private struct ArrayIndices
    {
        public int FirstIdx; // Index of first bitmap block used
        public int Count; // Number of bitmap blocks used
    }
}