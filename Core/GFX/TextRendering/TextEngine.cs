using System;
using System.Drawing;
using System.Numerics;
using SharpDX.Direct3D11;

namespace SpicyTemple.Core.GFX.TextRendering
{
    public class TextEngine
    {
        public TextEngine(Device mD3D11Device, bool debugDevice)
        {
            // TODO
        }

        public void SetScissorRect(int x, int y, int width, int height)
        {
            // TODO
        }

        public void ResetScissorRect()
        {
            // TODO
        }

        public void SetRenderTarget(Texture2D texture)
        {
           // TODO
        }

        public void RenderText(Rectangle rect, FormattedText formattedStr)
        {
            // TODO
        }

        public void RenderTextRotated(Rectangle rect, float angle, Vector2 center, FormattedText formattedStr)
        {
            // TODO
        }

        public void RenderText(Rectangle rect, TextStyle style, string text)
        {
            // TODO
        }
			
        public void MeasureText(FormattedText formattedStr, out TextMetrics metrics)
        {
            metrics = new TextMetrics();
            // TODO
        }

        public void MeasureText(TextStyle style, ReadOnlySpan<char> text, out TextMetrics metrics)
        {
            metrics = new TextMetrics();
            // TODO
        }

        public void AddFont(string filename)
        {
            // TODO
        }

        /**
         * Checks if this text engine can provide the given font family. If false, it means it would
         * use a fallback font.
         */
        public bool HasFontFamily(string name)
        {
            // TODO
            return false;
        }

        
    }
}