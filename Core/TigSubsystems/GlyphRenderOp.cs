
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using OpenTemple.Core.GFX;
using SkiaSharp;

namespace OpenTemple.Core.TigSubsystems
{
    /// <summary>
    /// This is all very slow, but it's only used for the Scurlock Bitmap font on the main menu.
    /// </summary>
    public class GlyphRenderOp : ICustomDrawOperation
    {
        private readonly SKImage _image;

        private readonly Rect _srcRect;

        private readonly Rect _destRect;

        private readonly PackedLinearColorA _topColor;

        private readonly PackedLinearColorA _bottomColor;

        public GlyphRenderOp(SKImage image, Rect srcRect, Rect destRect, PackedLinearColorA topColor,
            PackedLinearColorA bottomColor)
        {
            _image = image;
            _srcRect = srcRect;
            _destRect = destRect;
            _topColor = topColor;
            _bottomColor = bottomColor;
        }

        public void Render(IDrawingContextImpl context)
        {
            var srcRect = _srcRect.ToSKRect();
            var destRect = _destRect.ToSKRect();

            var skiaContext = (ISkiaDrawingContextImpl) context;
            var canvas = skiaContext.SkCanvas;

            using var paint = CreatePaint();
            canvas.DrawImage(_image, srcRect, destRect, paint);
        }

        private SKPaint CreatePaint()
        {
            var paint = new SKPaint();

            if (_topColor != _bottomColor)
            {
                // Use those offsets to create a gradient for the reflected text
                using var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, (float) _destRect.Top),
                    new SKPoint(0, (float) _destRect.Bottom),
                    new[]
                    {
                        _topColor.ToSkColor(),
                        _bottomColor.ToSkColor()
                    },
                    null,
                    SKShaderTileMode.Clamp);
                paint.Shader = shader;
            }
            else
            {
                paint.Color = _topColor.ToSkColor();
            }

            return paint;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool HitTest(Point p)
        {
            throw new System.NotImplementedException();
        }

        public Rect Bounds { get; }

        public bool Equals(ICustomDrawOperation? other)
        {
            throw new System.NotImplementedException();
        }
    }

    internal static class ColorExtensions
    {
        internal static SKColor ToSkColor(this PackedLinearColorA color)
        {
            return new(color.R, color.G, color.B, color.A);
        }
    }
}