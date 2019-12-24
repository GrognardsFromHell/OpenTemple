using System;
using System.Drawing;

namespace OpenTemple.Core.GFX
{

    public static class RectangleExtensions {
        public static void FitInto(this Rectangle rectangle, Rectangle boundingRect) {
            /*
            Calculates the rectangle within the back buffer that the scene
            will be drawn in. This accounts for "fit to width/height" scenarios
            where the back buffer has a different aspect ratio.
            */
            float w = boundingRect.Width;
            float h = boundingRect.Height;
            float wFactor = (float)w / rectangle.Width;
            float hFactor = (float)h / rectangle.Height;
            float scale = MathF.Min(wFactor, hFactor);
            rectangle.Width = (int)MathF.Round(scale * rectangle.Width);
            rectangle.Height = (int)MathF.Round(scale * rectangle.Height);

            // Center in bounding Rect
            rectangle.X = boundingRect.X + (boundingRect.Width - rectangle.Width) / 2;
            rectangle.Y = boundingRect.Y + (boundingRect.Height - rectangle.Height) / 2;
        }
    }

}