using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenTemple.Core.GameObjects;

public interface ISparseArray
{
    void WriteTo(BinaryWriter writer);

    ISparseArray Copy();

    int Count { get; }
}

public class SparseArray<T> : IReadOnlyList<T>, ISparseArray, IDisposable where T : struct
{
    private static readonly MemoryPool<T> Pool = MemoryPool<T>.Shared;

    private IMemoryOwner<T> _memory;

    private ArrayIdxMapId _idxBitmapId = ArrayIdxMapId.Null;

    public int Count { get; private set; }

    /// <summary>
    /// Returns a new empty array of this type.
    /// </summary>
    public static SparseArray<T> Empty => new();

    public bool HasIndex(int index)
    {
        if (_memory == null || !_idxBitmapId.IsValid)
        {
            // The type of the object didn't support this field
            return false;
        }

        return ArrayIndexBitmaps.Instance.HasIndex(_idxBitmapId, index);
    }

    public T this[int index]
    {
        get
        {
            if (!HasIndex(index))
            {
                return default;
            }

            var packedIdx = GetPackedIndex(index);
            return _memory.Memory.Span[packedIdx];
        }
        set
        {
            int packedIdx;

            // Add the corresponding index position
            if (!HasIndex(index))
            {
                if (!_idxBitmapId.IsValid)
                {
                    _idxBitmapId = ArrayIndexBitmaps.Instance.Allocate();
                }

                ArrayIndexBitmaps.Instance.AddIndex(_idxBitmapId, index);

                // Resize or allocate the array storage
                if (_memory == null || _memory.Memory.Length < Count + 1)
                {
                    var newMemory = Pool.Rent(Count + 1);
                    if (_memory != null)
                    {
                        _memory.Memory.Slice(0, Count).CopyTo(newMemory.Memory);
                        _memory.Dispose(); // Return it to the pool
                    }

                    _memory = newMemory;
                }

                packedIdx = GetPackedIndex(index);

                // Move back everything behind the packed Idx
                var data = _memory.Memory.Span;
                for (var i = Count; i > packedIdx; --i)
                {
                    data[i] = data[i - 1];
                }

                Count++;
            }
            else
            {
                packedIdx = GetPackedIndex(index);
            }

            _memory.Memory.Span[packedIdx] = value;
        }
    }

    public void Remove(int index)
    {
        if (_memory == null || !HasIndex(index))
        {
            return; // No storage allocated or index already removed
        }

        // If the array only consist of the element we are removing, deallocate it
        if (Count == 1)
        {
            Clear();
            return;
        }

        var storageIndex = GetPackedIndex(index);
        ArrayIndexBitmaps.Instance.RemoveIndex(_idxBitmapId, index);

        var data = _memory.Memory.Span;
        // Copy all the data from the back one place forward
        for (var i = storageIndex; i < Count - 1; ++i)
        {
            data[i] = data[i + 1];
        }

        Count--;

        // If we're using less than 2/3 capacity, let's try to downsize
        if (Count < 2 * _memory.Memory.Length / 3)
        {
            var newMemory = Pool.Rent(Count);
            // If the new memory isn't actually smaller, don't bother
            if (newMemory.Memory.Length >= _memory.Memory.Length)
            {
                newMemory.Dispose();
                return;
            }

            _memory.Memory.Slice(0, Count).CopyTo(newMemory.Memory);
            _memory.Dispose(); // Return it to the pool
            _memory = newMemory;
        }
    }

    public void Clear()
    {
        // Return the memory to the pool
        if (_memory != null)
        {
            _memory.Dispose();
            _memory = null;
        }

        if (_idxBitmapId.IsValid)
        {
            ArrayIndexBitmaps.Instance.Free(_idxBitmapId);
            _idxBitmapId = ArrayIdxMapId.Null;
        }

        Count = 0;
    }

    /**
         * Calls the given callback for every stored index in the array.
         * Also passes a mutable data pointer.
         */
    public void ForEachIndex(Action<int> callback)
    {
        if (!_idxBitmapId.IsValid)
        {
            return;
        }

        ArrayIndexBitmaps.Instance.ForEachIndex(_idxBitmapId, realIdx =>
        {
            callback(realIdx);
            return true;
        });
    }

    public void Append(T value)
    {
        this[Count] = value;
    }

    public void Dispose()
    {
        Clear();
    }

    private int GetPackedIndex(int index)
    {
        return ArrayIndexBitmaps.Instance.GetPackedIndex(_idxBitmapId, index);
    }

    private static readonly int ElementSize = Marshal.SizeOf<T>();

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(ElementSize);
        writer.Write(Count);
        writer.Write(0); // Was previously the array index id, but this is transient data

        // Create a raw byte-view of the underlying array
        if (_memory != null)
        {
            var rawData = MemoryMarshal.Cast<T, byte>(_memory.Memory.Span.Slice(0, Count));
            writer.Write(rawData);
        }

        // Save the array index bitmap as well
        ArrayIndexBitmaps.Instance.SerializeToStream(_idxBitmapId, writer);
    }

    public static SparseArray<T> ReadFrom(BinaryReader reader)
    {
        var elementSize = reader.ReadInt32();
        var count = reader.ReadInt32();
        reader.ReadInt32(); // Unused transient data

        if (elementSize != ElementSize)
        {
            throw new Exception($"Read element size {elementSize} from stream, but " +
                                $"expected {ElementSize}");
        }

        var result = new SparseArray<T>();
        result.Count = count;

        // Create a raw byte-view of the underlying array
        if (count > 0)
        {
            result._memory = Pool.Rent(count);

            var rawData = MemoryMarshal.Cast<T, byte>(result._memory.Memory.Span.Slice(0, count));
            if (reader.Read(rawData) != rawData.Length)
            {
                throw new IOException("Failed to read sparse array of size " + rawData.Length);
            }
        }

        result._idxBitmapId = ArrayIndexBitmaps.Instance.DeserializeFromFile(reader);

        return result;
    }

    public ISparseArray Copy()
    {
        var result = new SparseArray<T>();
        if (_memory != null)
        {
            result._memory = Pool.Rent(Count);
            var resultSpan = result._memory.Memory.Span;
            var sourceSpan = _memory.Memory.Slice(0, Count).Span;
            sourceSpan.CopyTo(resultSpan);
        }

        result.Count = Count;
        if (_idxBitmapId.IsValid)
        {
            result._idxBitmapId = ArrayIndexBitmaps.Instance.Clone(_idxBitmapId);
        }

        return result;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}