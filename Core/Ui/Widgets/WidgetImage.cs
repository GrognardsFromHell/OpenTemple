using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Widgets;

[Obsolete]
public class WidgetImage : WidgetContent, IDisposable
{
    private string? _path;

    private OptionalResourceRef<ITexture> _texture;
    
    public WidgetImage(string? path)
    {
        SetTexture(path);
    }

    public WidgetImage()
    {
    }

    public Rectangle? SourceRect { get; set; }

    public PackedLinearColorA Color { get; set; } = PackedLinearColorA.White;

    public override void Render(PointF origin)
    {
        var texture = _texture.Resource;
        if (texture == null)
        {
            return;
        }

        var renderer = Tig.ShapeRenderer2d;
        
        var destRect = ContentArea;
        destRect.Offset(origin);
        
        if (SourceRect.HasValue)
        {
            var drawArgs = new Render2dArgs();
            drawArgs.srcRect = SourceRect.Value;
            drawArgs.destRect = destRect;
            drawArgs.customTexture = texture;
            drawArgs.flags = Render2dFlag.BUFFERTEXTURE;
            if (Color != PackedLinearColorA.White)
            {
                drawArgs.flags |= Render2dFlag.VERTEXALPHA|Render2dFlag.VERTEXCOLORS;
                drawArgs.vertexColors = new[]
                {
                    Color,
                    Color,
                    Color,
                    Color
                };
            }
            renderer.DrawRectangle(ref drawArgs);
        }
        else
        {
            renderer.DrawRectangle(
                destRect,
                _texture.Resource,
                Color
            );
        }
    }

    public void SetTexture(string? path)
    {
        _path = path;
        _texture.Dispose();
        if (path != null)
        {
            _texture = Tig.RenderingDevice.GetTextures().Resolve(path, false);
            if (_texture.Resource?.IsValid() ?? false)
            {
                PreferredSize = _texture.Resource.GetSize();
            }
        }
    }

    public void SetTexture(ITexture texture)
    {
        _path = texture.GetName();
        _texture.Dispose();
        _texture = texture.Ref();
        if (_texture.Resource?.IsValid() ?? false)
        {
            PreferredSize = _texture.Resource.GetSize();
        }
    }
    
    public void Dispose()
    {
        _texture.Dispose();
    }
}
