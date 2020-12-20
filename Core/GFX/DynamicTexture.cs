using System;
using System.Drawing;
using Vortice.Direct3D11;

namespace OpenTemple.Core.GFX
{
    public class DynamicTexture : GpuResource<DynamicTexture>, ITexture
    {

        private readonly Size mSize;

        private readonly Rectangle mRectangle;

        private RenderingDevice _device;

        public DynamicTexture(RenderingDevice device,
            ID3D11Texture2D texture,
            ID3D11ShaderResourceView resourceView,
            Size size,
            int bytesPerPixel)
        {
            _device = device;
            mResourceView = resourceView;
            mTexture = texture;
            mSize = size;
            mRectangle = new Rectangle(Point.Empty, size);
            BytesPerPixel = bytesPerPixel;
        }

        internal ID3D11Texture2D mTexture;

        private ID3D11ShaderResourceView mResourceView;

        public int BytesPerPixel { get; }

        protected override void FreeResource()
        {
            FreeDeviceTexture();
        }

        public int GetId()
        {
            throw new NotSupportedException("Unsupported operation for dynamic textures.");
        }

        public string GetName() => "<dynamic>";

        public Rectangle GetContentRect() => mRectangle;

        public Size GetSize() => mSize;
        public void FreeDeviceTexture()
        {
            mTexture?.Dispose();
            mTexture = null;
            mResourceView?.Dispose();
            mResourceView = null;
        }

        public ID3D11ShaderResourceView GetResourceView() => mResourceView;

        public TextureType Type => TextureType.Dynamic;

        public bool IsValid() => true;

        public void UpdateRaw(ReadOnlySpan<byte> data, int pitch)
        {
            using var mapped = _device.Map(this);

            if (mapped.RowPitch == pitch) {
                data.CopyTo(mapped.Data);
            } else {
                var dest = mapped.Data;
                var rowWidth = Math.Min(pitch, mapped.RowPitch);
                for (var y = 0; y < mSize.Height; ++y)
                {
                    var srcRow = data.Slice(y * pitch, rowWidth);
                    var destRow = dest.Slice(y * mapped.RowPitch, rowWidth);
                    srcRow.CopyTo(destRow);
                }
            }
        }
    }
}