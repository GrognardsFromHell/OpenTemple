using System;
using SharpDX.Direct3D11;

namespace SpicyTemple.Core.GFX
{
    public ref struct MappedBuffer<T>
    {
        private Resource _resource;

        private DeviceContext _context;

        public int RowPitch { get; }

        public Span<T> Data { get; private set; }

        public MappedBuffer(Resource resource, DeviceContext context, Span<T> data, int rowPitch) : this()
        {
            _resource = resource;
            _context = context;
            Data = data;
            RowPitch = rowPitch;
        }

        public void Dispose()
        {
            if (_resource != null)
            {
                Data = Span<T>.Empty;

                _context.UnmapSubresource(_resource, 0);
                _resource = null;
                _context = null;
            }
        }
    }
}