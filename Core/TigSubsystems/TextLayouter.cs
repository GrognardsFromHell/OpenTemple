using System;
using System.Drawing;
using System.Numerics;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.IO.Fonts;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.TigSubsystems;

/// <summary>
/// Separates a block of text given flags into words split up
/// on lines and renders them.
/// </summary>
public class TextLayouter : IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly FontRenderer _renderer;
    private readonly ShapeRenderer2d _shapeRenderer;

    public TextLayouter(RenderingDevice device, ShapeRenderer2d shapeRenderer)
    {
        _renderer = new FontRenderer(device);
        _shapeRenderer = shapeRenderer;
    }

    public void Dispose()
    {
        _renderer.Dispose();
    }

    public void LayoutAndDraw(ReadOnlySpan<char> text, TigFont font, ref RectangleF extents, TigTextStyle style)
    {
        if (text.Length == 0)
        {
            return;
        }

        // Get the base text format and check if we should render using the new or old algorithms
        // Make the text mutable since vanilla drawing might change escape characters
        // within the text span.
        Span<char> mutableText = stackalloc char[text.Length];
        text.CopyTo(mutableText);

        // use the old font drawing algorithm
        LayoutAndDrawVanilla(mutableText, font, ref extents, style);
    }

    public void Measure(TigFont font, TigTextStyle style, ReadOnlySpan<char> text, ref TigFontMetrics metrics)
    {
        // use the old font drawing algorithm
        MeasureVanilla(font, style, text, ref metrics);
    }

    // TODO I believe this function measures how many characters will fit into the current line given the bounds.
    public int MeasureLineWrap(TigFont font, TigTextStyle style, ReadOnlySpan<char> text, Rectangle bounds)
    {
        if (bounds.Width == 0)
        {
            return text.Length;
        }

        Span<char> textCopy = stackalloc char[text.Length];
        text.CopyTo(textCopy);

        var iterator = new LayoutRunIterator(textCopy, font, bounds, style);
        int endOfLine = 0;

        while (iterator.MoveToNextRun(out var run))
        {
            if (run.Y <= bounds.Y)
            {
                endOfLine = run.End;
            }
            else
            {
                break;
            }
        }

        return endOfLine;
    }

    private void DrawBackgroundOrOutline(RectangleF rect, TigTextStyle style)
    {
        float left = rect.X;
        float top = rect.Y;
        var right = left + rect.Width;
        var bottom = top + rect.Height;

        left -= 3;
        top -= 3;
        right += 3;
        bottom += 3;

        if (style.flags.HasFlag(TigTextStyleFlag.Background))
        {
            Span<Vertex2d> corners = stackalloc Vertex2d[4];
            corners[0].pos = new Vector4(left, top, 0.5f, 1);
            corners[1].pos = new Vector4(right, top, 0.5f, 1);
            corners[2].pos = new Vector4(right, bottom, 0.5f, 1);
            corners[3].pos = new Vector4(left, bottom, 0.5f, 1);

            corners[0].diffuse = style.bgColor.TopLeft;
            corners[1].diffuse = style.bgColor.TopRight;
            corners[2].diffuse = style.bgColor.BottomRight;
            corners[3].diffuse = style.bgColor.BottomLeft;

            corners[0].uv = Vector2.Zero;
            corners[1].uv = Vector2.Zero;
            corners[2].uv = Vector2.Zero;
            corners[3].uv = Vector2.Zero;

            // Draw an untextured rectangle
            _shapeRenderer.DrawRectangle(corners, null);
        }

        if (style.flags.HasFlag(TigTextStyleFlag.Border))
        {
            _shapeRenderer.DrawRectangleOutline(
                RectangleF.FromLTRB(
                    left - 1,
                    top - 1,
                    right + 1,
                    bottom + 1
                ),
                PackedLinearColorA.Black
            );
        }
    }

    internal static ScanWordResult ScanWord(Span<char> text,
        int firstIdx,
        int textLength,
        bool lastLine,
        TigFont font,
        TigTextStyle style,
        float remainingSpace)
    {
        var result = new ScanWordResult();
        result.firstIdx = firstIdx;

        var glyphs = font.FontFace.Glyphs;

        var i = firstIdx;
        for (; i < textLength; i++)
        {
            var curCh = text[i];
            var nextCh = '\0';
            if (i + 1 < textLength)
            {
                nextCh = text[i + 1];
            }

            if (curCh == '’')
            {
                curCh = text[i] = '\'';
            }

            // Simply skip @t without increasing the width
            if (curCh == '@' && char.IsDigit(nextCh))
            {
                i++; // Skip the number
                continue;
            }

            if (!font.GetGlyphIdx(curCh, out var glyphIdx))
            {
                Logger.Warn("Tried to display character {0} in text '{1}'", glyphIdx, new string(text));
                continue;
            }

            if (curCh == '\n')
            {
                if (lastLine && style.flags.HasFlag(TigTextStyleFlag.Truncate))
                {
                    result.drawEllipsis = true;
                }

                break;
            }

            if (curCh < 128 && curCh > 0 && char.IsWhiteSpace(curCh))
            {
                break;
            }

            if (style.flags.HasFlag(TigTextStyleFlag.Truncate))
            {
                result.fullWidth += glyphs[glyphIdx].WidthLine + style.kerning;
                if (result.fullWidth > remainingSpace)
                {
                    result.drawEllipsis = true;
                    continue;
                }

                result.idxBeforePadding = i;
            }

            result.Width += glyphs[glyphIdx].WidthLine + style.kerning;
        }

        result.lastIdx = i;
        return result;
    }

    internal static Tuple<int, int> MeasureCharRun(ReadOnlySpan<char> text,
        TigTextStyle style,
        RectangleF extents,
        float extentsWidth,
        TigFont font,
        float linePadding,
        bool lastLine)
    {
        var lineWidth = 0;
        var wordCountWithPadding = 0;
        var wordWidth = 0;
        var wordCount = 0;

        var glyphs = font.FontFace.Glyphs;

        // This seems to be special handling for the sequence "@t" and @0 - @9
        var index = 0;
        for (; index < text.Length; ++index)
        {
            var ch = text[index];
            var nextCh = '\0';
            if (index + 1 < text.Length)
            {
                nextCh = text[index + 1];
            }

            // Handles @0 to @9
            if (ch == '@' & char.IsDigit(nextCh))
            {
                ++index; // Skip the number
            }
            else if (ch == '\n')
            {
                if (lineWidth + wordWidth <= extentsWidth)
                {
                    wordCount++;
                    if (lineWidth + wordWidth <= extentsWidth + linePadding)
                    {
                        wordCountWithPadding++;
                    }

                    lineWidth += wordWidth;
                    wordWidth = 0;
                }

                break;
            }
            else if (ch < 255 && ch > -1 && char.IsWhiteSpace(ch))
            {
                if (lineWidth + wordWidth <= extentsWidth)
                {
                    wordCount++;
                    if (lineWidth + wordWidth <= extentsWidth + linePadding)
                    {
                        wordCountWithPadding++;
                    }

                    lineWidth += wordWidth + style.tracking;
                    wordWidth = 0;
                }
                else
                {
                    // Stop if we have run out of space on this line
                    break;
                }
            }
            else if (ch == '’') // special casing this motherfucker
            {
                ch = '\'';
                if (font.GetGlyphIdx(ch, out var glyphIdx))
                {
                    wordWidth += style.kerning + glyphs[glyphIdx].WidthLine;
                }
            }
            else
            {
                if (font.GetGlyphIdx(ch, out var glyphIdx))
                {
                    wordWidth += style.kerning + glyphs[glyphIdx].WidthLine;
                }
            }
        }

        // Handle the last word, if we're at the end of the string
        if (index >= text.Length && wordWidth > 0)
        {
            if (lineWidth + wordWidth <= extentsWidth)
            {
                wordCount++;
                lineWidth += wordWidth;
                if (lineWidth + wordWidth <= extentsWidth + linePadding)
                {
                    wordCountWithPadding++;
                }
            }
            else if (style.flags.HasFlag(TigTextStyleFlag.Truncate))
            {
                // The word would actually not fit, but we're the last
                // thing in the string and we truncate with ...
                lineWidth += wordWidth;
                wordCount++;
                wordCountWithPadding++;
            }
        }

        // Ignore the padding if we'd not print ellipsis anyway
        if (!lastLine || index >= text.Length || !style.flags.HasFlag(TigTextStyleFlag.Truncate))
        {
            wordCountWithPadding = wordCount;
        }

        return Tuple.Create(wordCountWithPadding, lineWidth);
    }

    internal static bool HasMoreText(ReadOnlySpan<char> text)
    {
        // We're on the last line and truncation is active
        // This will seek to the next word
        for (var index = 0; index < text.Length; ++index)
        {
            var curChar = text[index];
            var nextChar = '\0';
            if (index + 1 < text.Length)
            {
                nextChar = text[index + 1];
            }

            // Handles @0 - @9 and skips the number
            if (curChar == '@' && char.IsDigit(nextChar))
            {
                ++index;
                continue;
            }

            if (curChar != '\n' && !char.IsWhiteSpace(curChar))
            {
                return true;
            }
        }

        return false;
    }

    private void LayoutAndDrawVanilla(
        Span<char> text,
        TigFont font,
        ref RectangleF extents,
        TigTextStyle style)
    {
        var extentsWidth = extents.Width;
        var extentsHeight = extents.Height;
        if (extentsWidth == 0)
        {
            var metrics = new TigFontMetrics();
            metrics.width = extents.Width;
            metrics.height = extents.Height;
            Tig.Fonts.Measure(style, text, ref metrics);

            extents.Width = metrics.width;
            extents.Height = metrics.height;
            extentsWidth = metrics.width;
            extentsHeight = metrics.height;
        }

        if ((style.flags & (TigTextStyleFlag.Background | TigTextStyleFlag.Border)) != 0)
        {
            var rect = new RectangleF(
                extents.X,
                extents.Y,
                Math.Max(extentsWidth, extents.Width),
                Math.Max(extentsHeight, extents.Height)
            );
            DrawBackgroundOrOutline(rect, style);
        }

        var iterator = new LayoutRunIterator(text, font, extents, style);
        while (iterator.MoveToNextRun(out var run))
        {
            if (run.Truncated)
            {
                _renderer.RenderRun(
                    "...",
                    run.X,
                    run.Y,
                    run.Bounds,
                    style,
                    font);
            }
            else
            {
                _renderer.RenderRun(
                    text.Slice(run.Start, run.End - run.Start),
                    run.X,
                    run.Y,
                    run.Bounds,
                    style,
                    font);
            }
        }
    }

    [TempleDllLocation(0x101ea4e0)]
    private void MeasureVanilla(TigFont font,
        TigTextStyle style,
        ReadOnlySpan<char> text,
        ref TigFontMetrics metrics)
    {
        if (metrics.width == 0 && text.Contains('\n'))
        {
            metrics.width = MeasureVanillaParagraph(font, style, text);
        }

        var largestHeight = font.FontFace.LargestHeight;
        if (metrics.width == 0)
        {
            metrics.width = MeasureVanillaLine(font, style, text);
            metrics.height = largestHeight;
            metrics.lines = 1;
            metrics.lineheight = largestHeight;
            return;
        }

        metrics.lines = 1; // Default
        if (metrics.height != 0)
        {
            var maxLines = (int) metrics.height / largestHeight;
            if (!style.flags.HasFlag(TigTextStyleFlag.Truncate))
            {
                maxLines++;
            }

            if (maxLines != 1)
            {
                metrics.lines = CountLinesVanilla(metrics.width, maxLines, text, font, style);
            }
        }
        else
        {
            if (!(style.flags.HasFlag(TigTextStyleFlag.Truncate)))
            {
                metrics.lines = CountLinesVanilla(metrics.width, 0, text, font, style);
            }
        }

        if (metrics.height == 0)
        {
            metrics.height = metrics.lines * largestHeight;
            metrics.height -= -(font.FontFace.BaseLine - largestHeight);
        }

        metrics.lineheight = largestHeight;
    }

    private int MeasureVanillaLine(TigFont font, TigTextStyle style, ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
        {
            return 0;
        }

        var result = 0;
        var length = text.Length;
        var glyphs = font.FontFace.Glyphs;

        for (var i = 0; i < length; i++)
        {
            var ch = text[i];

            // Skip @ characters if they are followed by a number between 0 and 9
            if (ch == '@' & i + 1 < length && text[i + 1] >= '0' && text[i + 1] <= '9')
            {
                i++;
                continue;
            }

            if (ch >= 0 && ch < 128 && char.IsWhiteSpace(ch))
            {
                if (ch != '\n')
                {
                    result += style.tracking;
                }
            }
            else
            {
                if (font.GetGlyphIdx(ch, out var glyphIdx))
                {
                    result += glyphs[glyphIdx].WidthLine + style.kerning;
                }
            }
        }

        return result;
    }

    private int MeasureVanillaParagraph(TigFont font, TigTextStyle style, ReadOnlySpan<char> text)
    {
        Span<char> tempText = stackalloc char[text.Length + 1];
        text.CopyTo(tempText);
        tempText[text.Length] = '\n';

        var maxLineLen = 0;

        Span<char> textRest = tempText;
        var nextNewline = textRest.IndexOf('\n');
        while (nextNewline != -1)
        {
            var currentLine = textRest.Slice(0, nextNewline);
            textRest = textRest.Slice(nextNewline + 1);
            var lineLen = MeasureVanillaLine(font, style, currentLine);
            if (lineLen > maxLineLen)
            {
                maxLineLen = lineLen;
            }

            nextNewline = textRest.IndexOf('\n');
        }

        return maxLineLen;
    }

    private int CountLinesVanilla(float maxWidth, int maxLines, ReadOnlySpan<char> text, TigFont font,
        TigTextStyle style)
    {
        var length = text.Length;

        if (length <= 0)
            return 1;

        var lineWidth = 0;
        var lines = 1;

        var glyphs = font.FontFace.Glyphs;

        var ch = '\0';
        for (var i = 0; i < length; i++)
        {
            var wordWidth = 0;

            // Measure the length of the current word
            for (; i < length; i++)
            {
                ch = text[i];
                if (ch == '’') // fix for this character that sometimes appears in vanilla
                    ch = '\'';
                // Skip @[0-9]
                if (ch == '@' & i + 1 < length && text[i + 1] >= '0' && text[i + 1] <= '9')
                {
                    i++;
                    continue;
                }


                if (ch < 255 && ch >= 0)
                {
                    if (char.IsWhiteSpace(ch))
                    {
                        break;
                    }
                }

                if (font.GetGlyphIdx(ch, out var glyphIdx))
                {
                    wordWidth += glyphs[glyphIdx].WidthLine + style.kerning;
                }
            }

            lineWidth += wordWidth;

            // If there's enough space in the maxWidth left and we're not at a newline
            // increase the linewidth and continue on.
            if (lineWidth <= maxWidth && ch != '\n')
            {
                if (ch < 255 && ch >= 0 && char.IsWhiteSpace(ch))
                {
                    lineWidth += style.tracking;
                }

                continue;
            }

            // We're either at a newline, or break the line here due to reaching the maxwidth
            lines++;

            // Reached the max number of lines . quit
            if (maxLines != 0 && lines >= maxLines)
            {
                break;
            }

            if (lineWidth <= maxWidth)
            {
                // We reached a normal line break
                lineWidth = 0;
            }
            else
            {
                // We're breaking the line, so we'll keep the current word
                // width as the initial length of the new line
                lineWidth = wordWidth;
            }

            // Continuation indent
            if (style.flags.HasFlag(TigTextStyleFlag.ContinuationIndent))
            {
                lineWidth += 8 * style.tracking;
            }

            if (ch < 255 && ch >= 0 && char.IsWhiteSpace(ch))
            {
                if (ch != '\n')
                {
                    lineWidth += style.tracking;
                }
            }
        }

        return lines;
    }
}