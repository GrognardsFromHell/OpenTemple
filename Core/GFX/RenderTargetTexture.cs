using System;
using System.Drawing;
using SharpDX.Direct3D11;

namespace OpenTemple.Core.GFX;

public class RenderTargetTexture : GpuResource<RenderTargetTexture>, ITexture
{
    private readonly RenderTargetView _rtView;
    private readonly Texture2D _texture;
    private readonly Texture2D? _resolvedTexture;
    private readonly ShaderResourceView? _resourceView;
    private readonly Size _size;
    private readonly Rectangle _contentRect;
    private readonly bool _multiSampled;

    internal Texture2D Texture => _texture;

    internal RenderTargetView RenderTargetView => _rtView;

    public bool IsMultiSampled => _multiSampled;

    /// <summary>
    /// Only valid for MSAA targets. Will return the texture used to resolve the multisampling
    /// so it can be used as a shader resource.
    /// </summary>
    public Texture2D? ResolvedTexture => _resolvedTexture;

    /// <summary>
    /// For MSAA targets, this will return the resolvedTexture, while for normal targets,
    /// this will just be the texture itself.
    /// </summary>
    public ShaderResourceView? ResourceView => _resourceView;

    public BufferFormat Format { get; }

    public RenderTargetTexture(RenderingDevice device,
        Texture2D texture,
        RenderTargetView rtView,
        Texture2D? resolvedTexture,
        ShaderResourceView? resourceView,
        Size size,
        bool multisampled)
    {
        _texture = texture.QueryInterface<Texture2D>(); // Creates our own reference
        _rtView = rtView;
        _resolvedTexture = resolvedTexture;
        _resourceView = resourceView;
        _size = size;
        _multiSampled = multisampled;
        _contentRect = new Rectangle(Point.Empty, size);

        var desc = _texture.Description;
        switch (desc.Format)
        {
            case SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                Format = BufferFormat.A8R8G8B8;
                break;
            case SharpDX.DXGI.Format.B8G8R8X8_UNorm:
                Format = BufferFormat.X8R8G8B8;
                break;
            default:
                throw new GfxException("Unsupported buffer format: " + desc.Format);
        }
    }

    protected override void FreeResource()
    {
        _rtView.Dispose();
        _texture.Dispose();
        _resolvedTexture?.Dispose();
        _resourceView?.Dispose();
    }

    public int GetId()
    {
        throw new NotImplementedException("Unsupported operation for render target textures.");
    }

    public string GetName() => "<rt>";

    public Rectangle GetContentRect() => _contentRect;

    public Size GetSize() => _size;

    public void FreeDeviceTexture()
    {
        _texture.Dispose();
        _rtView.Dispose();
        _resourceView?.Dispose();
        _resolvedTexture?.Dispose();
    }

    public ShaderResourceView GetResourceView()
    {
        if (_resourceView == null)
        {
            throw new InvalidOperationException("This render target texture is not suitable" +
                                                " for use in a shader.");
        }

        return _resourceView;
    }

    public TextureType Type => TextureType.RenderTarget;

    public bool IsValid() => true;
}