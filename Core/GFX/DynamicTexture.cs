using System;
using System.Drawing;
using SharpDX.Direct3D11;

namespace SpicyTemple.Core.GFX
{
    public class DynamicTexture : GpuResource<DynamicTexture>, ITexture
    {

        private readonly Size mSize;

        private readonly ContentRect mContentRect;

        private RenderingDevice _device;

        public DynamicTexture(RenderingDevice device,
            Texture2D texture,
            ShaderResourceView resourceView,
            Size size,
            int bytesPerPixel)
        {
            _device = device;
            mResourceView = resourceView;
            mTexture = texture;
            mSize = size;
            mContentRect = new ContentRect(0, 0, size.Width, size.Height);
            BytesPerPixel = bytesPerPixel;
        }

        internal Texture2D mTexture;

        private ShaderResourceView mResourceView;

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

        public ContentRect GetContentRect() => mContentRect;

        public Size GetSize() => mSize;
        public void FreeDeviceTexture()
        {
            mTexture?.Dispose();
            mTexture = null;
            mResourceView?.Dispose();
            mResourceView = null;
        }

        public ShaderResourceView GetResourceView() => mResourceView;

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