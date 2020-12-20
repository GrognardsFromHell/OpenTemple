using System;
using System.Diagnostics;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace OpenTemple.Core.GFX
{
    public class IndexBuffer : GpuResource<IndexBuffer>
    {
        internal ID3D11Buffer Buffer { get; private set; }

        public Format Format { get; }

        internal int Count { get; }

        private readonly RenderingDevice _device;

        public IndexBuffer(RenderingDevice device, ID3D11Buffer buffer, Format format, int count)
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
        internal ID3D11Buffer Buffer { get; private set; }

        internal int Size { get; }

        private readonly RenderingDevice _device;

        public VertexBuffer(RenderingDevice device, ID3D11Buffer buffer, int size)
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
}