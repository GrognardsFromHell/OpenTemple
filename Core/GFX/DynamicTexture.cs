using System;
using System.Drawing;
using SharpDX.Direct3D11;

namespace OpenTemple.Core.GFX;

public class DynamicTexture : GpuResource<DynamicTexture>, ITexture
{
    private readonly Size _size;

    private readonly Rectangle _rectangle;

    private RenderingDevice _device;

    internal Texture2D _texture;

    private ShaderResourceView _resourceView;

    public int BytesPerPixel { get; }

    public DynamicTexture(RenderingDevice device,
        Texture2D texture,
        ShaderResourceView resourceView,
        Size size,
        int bytesPerPixel)
    {
        _device = device;
        _resourceView = resourceView;
        _texture = texture;
        _size = size;
        _rectangle = new Rectangle(Point.Empty, size);
        BytesPerPixel = bytesPerPixel;
    }

    protected override void FreeResource()
    {
        FreeDeviceTexture();
    }

    public int GetId()
    {
        throw new NotSupportedException("Unsupported operation for dynamic textures.");
    }

    public string GetName() => "<dynamic>";

    public Rectangle GetContentRect() => _rectangle;

    public Size GetSize() => _size;

    public void FreeDeviceTexture()
    {
        _texture.Dispose();
        _resourceView.Dispose();
    }

    public ShaderResourceView GetResourceView() => _resourceView;

    public TextureType Type => TextureType.Dynamic;

    public bool IsValid() => true;

    public void UpdateRaw(ReadOnlySpan<byte> data, int pitch)
    {
        using var mapped = _device.Map(this);

        if (mapped.RowPitch == pitch)
        {
            data.CopyTo(mapped.Data);
        }
        else
        {
            var dest = mapped.Data;
            var rowWidth = Math.Min(pitch, mapped.RowPitch);
            for (var y = 0; y < _size.Height; ++y)
            {
                var srcRow = data.Slice(y * pitch, rowWidth);
                var destRow = dest.Slice(y * mapped.RowPitch, rowWidth);
                srcRow.CopyTo(destRow);
            }
        }
    }
}