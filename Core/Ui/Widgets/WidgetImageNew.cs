using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetImageNew : WidgetBase
{
    private OptionalResourceRef<ITexture> _texture;

    public string? TexturePath
    {
        get => _texture.Resource?.GetName();
        set
        {
            _texture.Dispose();
            if (value != null)
            {
                _texture = Tig.RenderingDevice.GetTextures().Resolve(value, false);
            }
        }
    }

    public Rectangle? SourceRect { get; set; }

    public PackedLinearColorA Color { get; set; } = PackedLinearColorA.White;

    public WidgetImageNew([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1)
    {
    }

    protected override SizeF ComputePreferredPaddingAreaSize(float availableWidth, float availableHeight)
    {
        var contentSize = _texture.Resource?.GetSize() ?? SizeF.Empty;

        var styles = ComputedStyles;
        return new SizeF(
            contentSize.Width + styles.PaddingLeft + styles.PaddingRight,
            contentSize.Height + styles.PaddingTop + styles.PaddingBottom
        );
    }

    public override void Render(UiRenderContext context)
    {
        var contentArea = GetViewportContentArea();

        var texture = _texture.Resource;
        if (texture == null)
        {
            return;
        }

        var renderer = Tig.ShapeRenderer2d;
        if (SourceRect.HasValue)
        {
            var drawArgs = new Render2dArgs
            {
                srcRect = SourceRect.Value,
                destRect = contentArea,
                customTexture = texture,
                flags = Render2dFlag.BUFFERTEXTURE
            };
            if (Color != PackedLinearColorA.White)
            {
                drawArgs.flags |= Render2dFlag.VERTEXALPHA | Render2dFlag.VERTEXCOLORS;
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
                contentArea,
                _texture.Resource,
                Color
            );
        }
    }

    public void SetTexture(ITexture texture)
    {
        TexturePath = texture.GetName();
        _texture.Dispose();
        _texture = texture.Ref();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _texture.Dispose();
        }
    }
}