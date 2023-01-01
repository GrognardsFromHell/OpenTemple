using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.IO.Fonts;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.TigSubsystems;

internal ref struct LayoutRunIterator
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly Span<char> text;

    private readonly TigFont font;

    private readonly FontFaceGlyph[] glyphs;

    private readonly Rectangle extents;

    private readonly TigTextStyle style;

    private int ellipsisWidth;

    private int currentY;

    private int state;

    private bool lastLine;

    private int linePadding;

    // Used in for-loop
    private int startOfWord;

    private int wordsOnLine;

    private int lineWidth;

    // Which word on the line are we currently at
    private int wordIdx;

    private int currentX;

    private ScanWordResult wordInfo;

    private int wordWidth;

    private bool hasNextRun;

    internal LayoutRunIterator(Span<char> text,
        TigFont font,
        Rectangle extents,
        TigTextStyle style) : this()
    {
        this.text = text;
        this.font = font;
        this.extents = extents;
        this.style = style;
        glyphs = font.FontFace.Glyphs;
        state = 0;
    }

    internal bool MoveToNextRun(out LayoutRun nextRun)
    {
        switch (state)
        {
            default:
                throw new InvalidOperationException("Invalid state: " + state);
            case -1:
                nextRun = default;
                return false;
            case 0:
                currentY = extents.Y;

                lastLine = false;

                // TODO: Check if this can even happen since we measure the text
                // if the width hasn't been constrained
                if (extents.Width == 0)
                {
                    nextRun = new LayoutRun(
                        0,
                        text.Length,
                        extents.X,
                        extents.Y,
                        extents,
                        false
                    );
                    state = -1;
                    return true;
                }

                // Is there only space for one line?
                if (!font.GetGlyphIdx('.', out var dotIdx))
                {
                    throw new Exception("Font has no '.' character.");
                }

                ellipsisWidth = 3 * (style.kerning + glyphs[dotIdx].WidthLine);
                linePadding = 0;
                if (extents.Y + 2 * font.FontFace.LargestHeight > extents.Y + extents.Height)
                {
                    lastLine = true;
                    if ((style.flags & TigTextStyleFlag.Truncate) != 0)
                    {
                        linePadding = -ellipsisWidth;
                    }
                }

                if (text.Length <= 0)
                {
                    nextRun = default;
                    state = -1;
                    return false;
                }

                startOfWord = 0;
                state = 1;
                goto case 1;

            // Iterate one more character run
            case 1:
                if (startOfWord >= text.Length)
                {
                    state = -1;
                    nextRun = default;
                    return false;
                }

                (wordsOnLine, lineWidth) = TextLayouter.MeasureCharRun(
                    text[startOfWord..],
                    style,
                    extents,
                    extents.Width,
                    font,
                    linePadding,
                    lastLine);

                // There's just one word left and it wont fit. Remove restriction on width.
                if (wordsOnLine == 0 && (style.flags & TigTextStyleFlag.Truncate) == 0)
                {
                    (wordsOnLine, lineWidth) = TextLayouter.MeasureCharRun(
                        text[startOfWord..],
                        style,
                        extents,
                        9999999,
                        font,
                        linePadding,
                        lastLine);
                }

                currentX = 0;
                wordIdx = 0;
                state = 2;
                goto case 2;

            case 2:
                if (wordIdx >= wordsOnLine)
                {
                    // Advance to next line
                    currentY += font.FontFace.LargestHeight;
                    if (currentY + 2 * font.FontFace.LargestHeight > extents.Y + extents.Height)
                    {
                        lastLine = true;
                        if (style.flags.HasFlag(TigTextStyleFlag.Truncate))
                        {
                            linePadding = ellipsisWidth;
                        }
                    }

                    startOfWord++;
                    state = 1;
                    goto case 1;
                }

                var remainingSpace = extents.Width + linePadding - currentX;

                wordInfo = TextLayouter.ScanWord(text,
                    startOfWord,
                    text.Length,
                    lastLine,
                    font,
                    style,
                    remainingSpace);

                var lastIdx = wordInfo.lastIdx;
                wordWidth = wordInfo.Width;

                if (lastLine && (style.flags & TigTextStyleFlag.Truncate) != 0)
                {
                    if (currentX + wordInfo.fullWidth > extents.Width)
                    {
                        lastIdx = wordInfo.idxBeforePadding;
                    }
                    else
                    {
                        if (!TextLayouter.HasMoreText(text[lastIdx..]))
                        {
                            wordInfo.drawEllipsis = false;
                            wordWidth = wordInfo.fullWidth;
                        }
                    }
                }

                startOfWord = lastIdx;
                if (startOfWord < text.Length && text[startOfWord] >= 0 &&
                           char.IsWhiteSpace(text[startOfWord]))
                {
                    wordWidth += style.tracking;
                }

                // This means this is not the last word in this line
                if (wordIdx + 1 < wordsOnLine)
                {
                    startOfWord++;
                }

                // Draw the word
                var x = extents.X + currentX;
                if ((style.flags & TigTextStyleFlag.Center) != 0)
                {
                    x += (extents.Width - lineWidth) / 2;
                }

                if (wordInfo.firstIdx < 0 || lastIdx < 0)
                {
                    Logger.Error("Bad firstIdx at LayoutAndDraw! {0}, {1}", wordInfo.firstIdx, lastIdx);
                }
                else if (lastIdx >= wordInfo.firstIdx)
                {
                    nextRun = new LayoutRun(
                        wordInfo.firstIdx,
                        lastIdx,
                        x,
                        currentY,
                        extents,
                        false);
                    state = 3;
                    return true;
                }

                state = 3;
                goto case 3;

            case 3:
                currentX += wordWidth;

                // We're on the last line, the word has been truncated, ellipsis needs to be drawn
                if (lastLine && style.flags.HasFlag(TigTextStyleFlag.Truncate) &&
                    wordInfo.drawEllipsis)
                {
                    nextRun = new LayoutRun(
                        wordInfo.lastIdx,
                        wordInfo.lastIdx,
                        extents.X + currentX,
                        currentY,
                        extents,
                        true
                    );
                    state = -1;
                    return true;
                }

                wordIdx++;
                state = 2;
                goto case 2;

        }
    }
}

internal struct ScanWordResult
{
    public int firstIdx;
    public int lastIdx;
    public int idxBeforePadding;
    public int Width;
    public int fullWidth; // Ignores padding
    public bool drawEllipsis;
}

internal readonly struct LayoutRun
{
    public readonly int Start;
    public readonly int End;
    public readonly int X;
    public readonly int Y;
    public readonly Rectangle Bounds;
    public readonly bool Truncated;

    public LayoutRun(int start, int end, int x, int y, Rectangle bounds, bool truncated)
    {
        Start = start;
        End = end;
        X = x;
        Y = y;
        Bounds = bounds;
        Truncated = truncated;
    }
}