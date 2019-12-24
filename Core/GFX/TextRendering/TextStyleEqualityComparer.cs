using System;
using System.Collections.Generic;

namespace OpenTemple.Core.GFX.TextRendering
{
    internal class TextStyleEqualityComparer : IEqualityComparer<TextStyle>
    {
        public bool Equals(TextStyle a, TextStyle b)
        {
            if (!(a.fontFace == b.fontFace
                  && a.pointSize == b.pointSize
                  && a.bold == b.bold
                  && a.trim == b.trim
                  && a.italic == b.italic
                  && a.tabStopWidth == b.tabStopWidth
                  && a.align == b.align
                  && a.paragraphAlign == b.paragraphAlign
                  && a.uniformLineHeight == b.uniformLineHeight)) {
                return false;
            }
            if (a.uniformLineHeight) {
                return a.lineHeight == b.lineHeight
                       || a.baseLine == b.baseLine;
            }
            return true;
        }

        public int GetHashCode(TextStyle style)
        {
            var hash = new HashCode();
            hash.Add(style.fontFace);
            hash.Add(style.bold);
            hash.Add(style.trim);
            hash.Add(style.italic);
            hash.Add(style.pointSize);
            hash.Add(style.tabStopWidth);
            hash.Add(style.align);
            hash.Add(style.paragraphAlign);
            hash.Add(style.uniformLineHeight);

            if (style.uniformLineHeight)
            {
                hash.Add(style.lineHeight);
                hash.Add(style.baseLine);
            }

            return hash.ToHashCode();
        }
    }
}