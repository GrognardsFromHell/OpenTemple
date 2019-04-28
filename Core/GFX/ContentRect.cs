using System;
using System.Drawing;

namespace SpicyTemple.Core.GFX
{
    public struct ContentRect {
        public int x;
        public int y;
        public int width;
        public int height;

        public ContentRect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public ContentRect(int x, int y, Size size) : this()
        {
            this.x = x;
            this.y = y;
            this.width = size.Width;
            this.height = size.Height;
        }

        public void FitInto(ContentRect boundingRect) {

            /*
            Calculates the rectangle within the back buffer that the scene
            will be drawn in. This accounts for "fit to width/height" scenarios
            where the back buffer has a different aspect ratio.
            */
            float w = boundingRect.width;
            float h = boundingRect.height;
            float wFactor = (float)w / width;
            float hFactor = (float)h / height;
            float scale = MathF.Min(wFactor, hFactor);
            width = (int)MathF.Round(scale * width);
            height = (int)MathF.Round(scale * height);

            // Center in bounding Rect
            x = boundingRect.x + (boundingRect.width - width) / 2;
            y = boundingRect.y + (boundingRect.height - height) / 2;
        }

    }
}