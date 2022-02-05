using System;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.Fonts;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.TigSubsystems;

[Flags]
public enum TigTextStyleFlag
{
    TTSF_DROP_SHADOW = 0x8,
    TTSF_CENTER = 0x10,
    TTSF_CONTINUATION_INDENT = 0x200,
    TTSF_BACKGROUND = 0x400,
    TTSF_BORDER = 0x800,
    TTSF_TRUNCATE = 0x4000,
    TTSF_ROTATE = 0x8000,
    TTSF_ROTATE_OFF_CENTER = 0x10000
}

public struct ColorRect
{
    public PackedLinearColorA topLeft;
    public PackedLinearColorA topRight;
    public PackedLinearColorA bottomLeft;
    public PackedLinearColorA bottomRight;

    public ColorRect(PackedLinearColorA fill)
    {
        topLeft = fill;
        topRight = fill;
        bottomLeft = fill;
        bottomRight = fill;
    }
}

/**
 * Describes how text is rendered on the screen (style, font, etc.)
 */
public class TigTextStyle
{
    public int field0 = 0;
    public int tracking = 0;
    public int kerning = 1; // Anything less than this doesn't render properly
    public int leading = 0;
    public int field10 = 0;
    public int field14 = 0;
    public float rotation = 0;
    public float rotationCenterX = 0;
    public float rotationCenterY = 0;

    public int field24 = 0;

    /*
        8 seems to be drop shadow
        0x10 centers the text
        0x200 continuation indent
        0x400 Draws a filled rect behind the text (see bgColor)
        0x800 Draws a border box around the text, always black
        0xC00 is used for tooltips
        Not seen in the wild:
        0x1000
        0x2000
        0x4000 truncates text if too long for rect and appends "..."
        0x8000 seems to rotate
        0x10000 offset for rotation is set
    */
    public TigTextStyleFlag flags = 0;
    public int field2c = 0;
    public int colorSlot = 0;
    public ColorRect? textColor = null; // array of text colors for use with the @n text color modifiers
    public ColorRect[] additionalTextColors = null;

    public ColorRect GetTextColor(int idx)
    {
        if (idx == 0 || additionalTextColors == null)
        {
            return textColor.Value;
        }

        return additionalTextColors[idx - 1];
    }
    public ColorRect? colors2 = null;
    public ColorRect? shadowColor = null; // Use with flags |= 0x8
    public ColorRect? colors4 = null;
    public ColorRect? bgColor = null; // Use with flags |= 0x400
    public int field48 = 0;
    public int tabStop = 0; /* TODO tabstop */

    public static TigTextStyle standardWhite => new(new ColorRect(PackedLinearColorA.White));

    public TigTextStyle()
    {
    }

    public TigTextStyle(ColorRect? color)
    {
        textColor = color;
    }

    public TigTextStyle Copy() => (TigTextStyle) MemberwiseClone();
}

/**
     * Plug in "text" and pass to Measure to get the on-screen measurements a blob of text
     * will have.
     */
public struct TigFontMetrics
{
    public int width;
    public int height;
    public int lines;
    public int lineheight;
}

public class TigFont : IDisposable
{
    public FontFace FontFace { get; }

    private readonly ResourceRef<ITexture>[] _textures;

    public TigFont(FontFace fontFace, ResourceRef<ITexture>[] textures)
    {
        Trace.Assert(fontFace.FontArtCount == textures.Length);
        FontFace = fontFace;
        _textures = textures;
    }

    public ITexture GetFontArt(int index) => _textures[index].Resource;

    public void Dispose()
    {
        _textures.DisposeAll();
    }

    public bool GetGlyphIdx(char ch, out int glyphIdx)
    {
        // First character found in the FNT files
        var FirstFontChar = '!';
        int FirstNonEnglish = 0xa0;
        int FirstNonEnglishIdx = 92;

        var chUns = (int) ch;

        glyphIdx = chUns - FirstFontChar;

        if (chUns <= '~')
            return true;

        if (Tig.Fonts.FontIsEnglish)
        {
            if (chUns >= FirstNonEnglish)
            {
                glyphIdx = chUns - ((int) FirstNonEnglish - FirstNonEnglishIdx);
            }
            else
            {
                switch (chUns)
                {
                    case 0x82:
                        return GetGlyphIdx(',', out glyphIdx);
                    case 0x83:
                        return GetGlyphIdx('f', out glyphIdx);
                    case 0x84:
                        return GetGlyphIdx(',', out glyphIdx);
                    case 0x85: // elipsis
                        return GetGlyphIdx(';', out glyphIdx);
                    case 0x91:
                    case 0x92:
                        return GetGlyphIdx('\'', out glyphIdx);
                    case 0x93:
                    case 0x94:
                        return GetGlyphIdx('"', out glyphIdx);
                    case 0x95:
                        return GetGlyphIdx('·', out glyphIdx);
                    case 0x96:
                    case 0x97:
                        return GetGlyphIdx('-', out glyphIdx);
                    default:
                        return GetGlyphIdx('-', out glyphIdx); // speak english or die!!!
                }
            }

            if ((glyphIdx < -1 || ch > '~') & ch != '\n')
            {
                return false;
            }
        }

        return true;
    }


}