using System;
using Vortice.DirectWrite;

namespace OpenTemple.Core.GFX.TextRendering
{
    public sealed class TextBlock : IDisposable
    {
        private TextEngine _engine;

        private IDWriteTextLayout3 _layout;

        private string _text;

        internal TextBlock(TextEngine engine)
        {
            _engine = engine;
        }

        public void Dispose()
        {
            FreeLayout();
        }

        public void SetSelection(int from, int to)
        {
        }

        public void HideCaret()
        {
        }

        public void ShowCaret(int position)
        {
        }

        public void Render(int x, int y)
        {
            if (_layout == null)
            {
            }
        }

        private void ReleaseUnmanagedResources()
        {
        }

        public void SetText(string text)
        {
            FreeLayout();
            _text = text;
        }

        private void FreeLayout()
        {
            _layout?.Dispose();
            _layout = null;
        }
    }
}