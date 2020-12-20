using System;
using Vortice.Direct3D11;

namespace OpenTemple.Core.GFX
{
    public ref struct MappedBuffer<T>
    {
        private ID3D11Resource _resource;

        private ID3D11DeviceContext _context;

        public int RowPitch { get; }

        public Span<T> Data { get; private set; }

        public MappedBuffer(ID3D11Resource resource, ID3D11DeviceContext context, Span<T> data, int rowPitch) : this()
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

                _context.Unmap(_resource, 0);
                _resource = null;
                _context = null;
            }
        }
    }
}