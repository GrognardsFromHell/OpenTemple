using System;
using System.Buffers;
using System.IO;

namespace OpenTemple.Core.IO.TroikaArchives;

/// <summary>
/// A stream that is backed by an IMemoryOwner which will be freed when the
/// stream is disposed.
/// </summary>
public class VfsStream : Stream
{
    public IFileSystem FileSystem { get; }

    public string Path { get; }

    private IMemoryOwner<byte> _memoryOwner;

    private Memory<byte> _memory;

    private int _position;

    public VfsStream(IFileSystem fs, string path, IMemoryOwner<byte> memoryOwner)
    {
        FileSystem = fs;
        Path = path;
        _memoryOwner = memoryOwner;
        _memory = memoryOwner.Memory;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _memoryOwner?.Dispose();
            _memoryOwner = null;
            _memory = null;
        }
    }

    public Memory<byte> GetBuffer()
    {
        CheckNotDisposed();
        return _memory;
    }

    private void CheckNotDisposed()
    {
        if (_memoryOwner == null)
        {
            throw new ObjectDisposedException("The stream has already been disposed");
        }
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    public override long Length
    {
        get
        {
            CheckNotDisposed();
            return _memory.Length;
        }
    }

    public override long Position
    {
        get => _position;
        set
        {
            CheckNotDisposed();
            if (value < 0 || value >= _memory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _position = (int) value;
        }
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        CheckNotDisposed();
        int result = Math.Min(count, _memory.Length - _position);
        _memory.Slice(_position, result).CopyTo(buffer.AsMemory(offset, result));
        _position += result;
        return result;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        CheckNotDisposed();

        long target;
        try
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    target = offset;
                    break;

                case SeekOrigin.Current:
                    target = checked(offset + _position);
                    break;

                case SeekOrigin.End:
                    target = checked(offset + _memory.Length);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (target < 0 || target >= _memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        _position = (int) target;
        return target;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}