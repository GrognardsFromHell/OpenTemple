using System;
using System.Diagnostics;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace OpenTemple.Core.GFX;

public class IndexBuffer : GpuResource<IndexBuffer>
{
    internal Buffer Buffer { get; private set; }

    public Format Format { get; }

    internal int Count { get; }

    private readonly RenderingDevice _device;

    public IndexBuffer(RenderingDevice device, Buffer buffer, Format format, int count)
    {
        _device = device;
        Buffer = buffer;
        Format = format;
        Count = count;
    }

    public void Update(ReadOnlySpan<ushort> indices)
    {
        Trace.Assert(indices.Length == Count);
        _device.UpdateBuffer(this, indices);
    }

    protected override void FreeResource()
    {
        Buffer?.Dispose();
        Buffer = null;
    }
}

public class VertexBuffer : GpuResource<VertexBuffer>
{
    internal Buffer Buffer { get; private set; }

    internal int Size { get; }

    private readonly RenderingDevice _device;

    public VertexBuffer(RenderingDevice device, Buffer buffer, int size)
    {
        Buffer = buffer;
        Size = size;
        _device = device;
    }

    public void Update<T>(ReadOnlySpan<T> data) where T : struct
    {
        _device.UpdateBuffer(this, data);
    }

    protected override void FreeResource()
    {
        Buffer?.Dispose();
        Buffer = null;
    }
}