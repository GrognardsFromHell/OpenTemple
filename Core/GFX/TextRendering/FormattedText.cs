using System.Collections.Generic;

namespace SpicyTemple.Core.GFX.TextRendering
{
    public struct ConstrainedTextStyle
    {
        public TextStyle style;
        public int startChar;
        public int length;

        public ConstrainedTextStyle(TextStyle style) : this()
        {
            this.style = style;
        }
    }

    public struct FormattedText
    {
        public string text;
        public TextStyle defaultStyle;
        public List<ConstrainedTextStyle> formats;
    }
}
