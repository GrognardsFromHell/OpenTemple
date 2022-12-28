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
    DropShadow = 0x8,
    Center = 0x10,
    ContinuationIndent = 0x200,
    Background = 0x400,
    Border = 0x800,
    Truncate = 0x4000,
    Rotate = 0x8000,
    RotateOffCenter = 0x10000
}

public readonly record struct ColorRect(PackedLinearColorA TopLeft, PackedLinearColorA TopRight, PackedLinearColorA BottomLeft, PackedLinearColorA BottomRight)
{
    public static readonly ColorRect White = new(PackedLinearColorA.White);

    public ColorRect(PackedLinearColorA fill) : this(fill, fill, fill, fill)
    {
    }

    public static ColorRect GradientV(string topHex, string bottomHex) => GradientV(PackedLinearColorA.FromHex(topHex), PackedLinearColorA.FromHex(bottomHex));

    public static ColorRect GradientV(PackedLinearColorA top, PackedLinearColorA bottom) => new(top, top, bottom, bottom);
}

/**
 * Describes how text is rendered on the screen (style, font, etc.)
 */
public class TigTextStyle
{
    public int tracking = 0;
    public int kerning = 1; // Anything less than this doesn't render properly
    public float rotation = 0;
    public float rotationCenterX = 0;
    public float rotationCenterY = 0;

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
    public TigTextStyleFlag flags;
    public int colorSlot = 0;

    public ColorRect textColor = ColorRect.White;

    // array of text colors for use with the @n text color modifiers
    public ColorRect[]? additionalTextColors = null;

    public ColorRect GetTextColor(int idx)
    {
        if (idx == 0 || additionalTextColors == null)
        {
            return textColor;
        }

        return additionalTextColors[idx - 1];
    }

    public PackedLinearColorA shadowColor = PackedLinearColorA.Black; // Use with flags |= 0x8
    public ColorRect bgColor = ColorRect.White; // Use with flags |= 0x400

    public TigTextStyle Copy() => (TigTextStyle) MemberwiseClone();
}

/**
     * Plug in "text" and pass to Measure to get the on-screen measurements a blob of text
     * will have.
     */
public struct TigFontMetrics
{
    public float width;
    public float height;
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