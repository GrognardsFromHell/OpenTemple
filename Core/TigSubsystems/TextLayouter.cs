using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.TextRendering;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.TigSubsystems
{
    /// <summary>
    /// Separates a block of text given flags into words split up
    /// on lines and renders them.
    /// </summary>
    public class TextLayouter : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public TextLayouter(RenderingDevice device, ShapeRenderer2d shapeRenderer)
        {
            mTextEngine = device.GetTextEngine();
            mRenderer = new FontRenderer(device);
            mShapeRenderer = shapeRenderer;
            mMapping = new FontsMapping();
        }

        public void Dispose()
        {
            mRenderer.Dispose();
        }

        public void LayoutAndDraw(ReadOnlySpan<char> text, TigFont font, ref Rectangle extents, TigTextStyle style)
        {
            if (text.Length == 0)
            {
                return;
            }

            // Get the base text format and check if we should render using the new or old algorithms
            if (!mMapping.TryGetMapping(font.FontFace.Name, out var textStyle))
            {
                // Make the text mutable since vanilla drawing might change escape characters
                // within the text span.
                Span<char> mutableText = stackalloc char[text.Length];
                text.CopyTo(mutableText);

                // use the old font drawing algorithm
                LayoutAndDrawVanilla(mutableText, font, ref extents, style);
                return;
            }

            // Use the new text engine style of drawing
            var tabPos = style.tabStop - extents.X;
            ApplyStyle(style, tabPos, textStyle);

            // If the string contains an @ symbol, we need to assume it's a legacy formatted string that
            // we need to parse into the new format.
            bool isLegacyFormattedStr = text.Contains('@');

            FormattedText formatted = new FormattedText();
            if (isLegacyFormattedStr)
            {
                formatted = ProcessString(textStyle, style, text);
            }
            else
            {
                formatted.text = new string(text);
                formatted.defaultStyle = textStyle;
            }

            // Determine the real text width/height if necessary
            if (extents.Width <= 0 || extents.Height <= 0)
            {
                var metrics = mTextEngine.MeasureText(formatted);
                if (extents.Width <= 0)
                {
                    extents.Width = metrics.width;
                }

                if (extents.Height <= 0)
                {
                    extents.Height = metrics.height;
                }
            }

            // Handle drawing of border/background
            if (style.flags.HasFlag(TigTextStyleFlag.TTSF_BACKGROUND) ||
                style.flags.HasFlag(TigTextStyleFlag.TTSF_BORDER))
            {
                DrawBackgroundOrOutline(extents, style);
            }

            // Dispatch based on applied rotation
            if (style.flags.HasFlag(TigTextStyleFlag.TTSF_ROTATE))
            {
                var angle = Angles.ToDegrees(style.rotation);
                var center = Vector2.Zero;
                if (style.flags.HasFlag(TigTextStyleFlag.TTSF_ROTATE_OFF_CENTER))
                {
                    center.X = style.rotationCenterX;
                    center.Y = style.rotationCenterY;
                }

                mTextEngine.RenderTextRotated(extents, angle, center, formatted);
            }
            else
            {
                mTextEngine.RenderText(extents, formatted);
            }
        }

        public void Measure(TigFont font, TigTextStyle style, ReadOnlySpan<char> text, ref TigFontMetrics metrics)
        {
            // Get the base text format and check if we should render using the new or old algorithms
            if (!mMapping.TryGetMapping(font.FontFace.Name, out var textStyle))
            {
                // use the old font drawing algorithm
                MeasureVanilla(font, style, text, ref metrics);
                return;
            }

            var tabPos = style.tabStop;
            textStyle = ApplyStyle(style, tabPos, textStyle);

            // Centering doesn't make sense for measuring if no width is given
            if (metrics.width == 0 && textStyle.align != TextAlign.Left)
            {
                textStyle.align = TextAlign.Left;
            }

            TextMetrics textMetrics;
            if (text.Contains('@'))
            {
                var formatted = ProcessString(textStyle, style, text);
                textMetrics = mTextEngine.MeasureText(formatted, metrics.width, metrics.height);
            }
            else
            {
                textMetrics = mTextEngine.MeasureText(textStyle, text, metrics.width, metrics.height);
            }

            metrics.width = textMetrics.width;
            metrics.height = textMetrics.height;
            metrics.lineheight = textMetrics.lineHeight;
            metrics.lines = textMetrics.lines;
        }

        private static void SkipSpaces(ReadOnlySpan<char> text, ref int index)
        {
            while (index < text.Length && text[index] == ' ')
            {
                index++;
            }
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

        private void DrawBackgroundOrOutline(Rectangle rect, TigTextStyle style)
        {
            float left = rect.X;
            float top = rect.Y;
            var right = left + rect.Width;
            var bottom = top + rect.Height;

            left -= 3;
            top -= 3;
            right += 3;
            bottom += 3;

            if (style.flags.HasFlag(TigTextStyleFlag.TTSF_BACKGROUND))
            {
                Span<Vertex2d> corners = stackalloc Vertex2d[4];
                corners[0].pos = new Vector4(left, top, 0.5f, 1);
                corners[1].pos = new Vector4(right, top, 0.5f, 1);
                corners[2].pos = new Vector4(right, bottom, 0.5f, 1);
                corners[3].pos = new Vector4(left, bottom, 0.5f, 1);

                if (style.bgColor.HasValue)
                {
                    var bgColor = style.bgColor.Value;
                    corners[0].diffuse = bgColor.topLeft;
                    corners[1].diffuse = bgColor.topRight;
                    corners[2].diffuse = bgColor.bottomRight;
                    corners[3].diffuse = bgColor.bottomLeft;
                }
                else
                {
                    foreach (ref var corner in corners)
                    {
                        corner.diffuse = PackedLinearColorA.White;
                    }
                }

                corners[0].uv = Vector2.Zero;
                corners[1].uv = Vector2.Zero;
                corners[2].uv = Vector2.Zero;
                corners[3].uv = Vector2.Zero;

                // Draw an untexture rectangle
                mShapeRenderer.DrawRectangle(corners, null);
            }

            if (style.flags.HasFlag(TigTextStyleFlag.TTSF_BORDER))
            {
                var topLeft = new Vector2(left - 1, top - 1);
                var bottomRight = new Vector2(right + 1, bottom + 1);

                mShapeRenderer.DrawRectangleOutline(
                    topLeft,
                    bottomRight,
                    new PackedLinearColorA(0, 0, 0, 255)
                );
            }
        }

        internal static ScanWordResult ScanWord(Span<char> text,
            int firstIdx,
            int textLength,
            bool lastLine,
            TigFont font,
            TigTextStyle style,
            int remainingSpace)
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

                // @t will advance the width up to the next tabstop, but only if a tabstop has been specified
                if (curCh == '@' && nextCh == 't' && style.tabStop > 0)
                {
                    break; // Treat it like whitespace
                }

                if (!font.GetGlyphIdx(curCh, out var glyphIdx))
                {
                    Logger.Warn("Tried to display character {0} in text '{1}'", glyphIdx, new string(text));
                    continue;
                }

                if (curCh == '\n')
                {
                    if (lastLine && style.flags.HasFlag(TigTextStyleFlag.TTSF_TRUNCATE))
                    {
                        result.drawEllipsis = true;
                    }

                    break;
                }

                if (curCh < 128 && curCh > 0 && char.IsWhiteSpace(curCh))
                {
                    break;
                }

                if (style.flags.HasFlag(TigTextStyleFlag.TTSF_TRUNCATE))
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
            Rectangle extents,
            int extentsWidth,
            TigFont font,
            int linePadding,
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
                else if (ch == '@' && nextCh == 't')
                {
                    ++index; // Skip the t

                    // Same handling as whitespaces, but variable sized increment rather than just adding tracking!
                    if (lineWidth + wordWidth <= extentsWidth)
                    {
                        wordCount++;
                        if (lineWidth + wordWidth <= extentsWidth + linePadding)
                        {
                            wordCountWithPadding++;
                        }

                        lineWidth += wordWidth;

                        // Increase the line width such that it continues at the tab stop location,
                        // but do not move backwards (unsupported)
                        lineWidth += Math.Max(0, style.tabStop - lineWidth);

                        wordWidth = 0;
                    }
                    else
                    {
                        // Stop if we have run out of space on this line
                        break;
                    }
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
                else if (style.flags.HasFlag(TigTextStyleFlag.TTSF_TRUNCATE))
                {
                    // The word would actually not fit, but we're the last
                    // thing in the string and we truncate with ...
                    lineWidth += wordWidth;
                    wordCount++;
                    wordCountWithPadding++;
                }
            }

            // Ignore the padding if we'd not print ellipsis anyway
            if (!lastLine || index >= text.Length || !style.flags.HasFlag(TigTextStyleFlag.TTSF_TRUNCATE))
            {
                wordCountWithPadding = wordCount;
            }

            return Tuple.Create(wordCountWithPadding, lineWidth);
        }

        internal static bool HasMoreText(ReadOnlySpan<char> text, bool skipTabs)
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

                if (curChar == '@' && nextChar == 't')
                {
                    ++index;
                    if (skipTabs)
                    {
                        continue;
                    }
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
            ref Rectangle extents,
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

            if ((style.flags & (TigTextStyleFlag.TTSF_BACKGROUND | TigTextStyleFlag.TTSF_BORDER)) != 0)
            {
                var rect = new Rectangle(
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
                    mRenderer.RenderRun(
                        "...",
                        run.X,
                        run.Y,
                        run.Bounds,
                        style,
                        font);
                }
                else
                {
                    mRenderer.RenderRun(
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
                var maxLines = metrics.height / largestHeight;
                if (!(style.flags.HasFlag(TigTextStyleFlag.TTSF_TRUNCATE)))
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
                if (!(style.flags.HasFlag(TigTextStyleFlag.TTSF_TRUNCATE)))
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

        private int CountLinesVanilla(int maxWidth, int maxLines, ReadOnlySpan<char> text, TigFont font,
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
                if (style.flags.HasFlag(TigTextStyleFlag.TTSF_CONTINUATION_INDENT))
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

        private static TextStyle ApplyStyle(TigTextStyle style, int tabPos, TextStyle textStyle)
        {
            var result = textStyle.Copy();

            if (tabPos > 0)
            {
                result.tabStopWidth = tabPos;
            }

            // Convert the color (optional for measurements)
            if (style.textColor.HasValue)
            {
                var textColor = style.textColor.Value;
                result.foreground.primaryColor = textColor.topLeft;
                if (textColor.topLeft != textColor.bottomRight)
                {
                    result.foreground.gradient = true;
                    result.foreground.secondaryColor = textColor.bottomRight;
                }
            }

            if (style.flags.HasFlag(TigTextStyleFlag.TTSF_TRUNCATE))
            {
                result.trim = true;
            }

            // Layouting options
            if (style.flags.HasFlag(TigTextStyleFlag.TTSF_CENTER))
            {
                result.align = TextAlign.Center;
            }

            if (style.flags.HasFlag(TigTextStyleFlag.TTSF_DROP_SHADOW))
            {
                result.dropShadow = true;
                if (style.shadowColor.HasValue)
                {
                    result.dropShadowBrush.primaryColor = style.shadowColor.Value.topLeft;
                    result.dropShadowBrush.primaryColor.A = 255;
                }
            }

            return result;
        }

        private static FormattedText ProcessString(TextStyle defaultStyle, TigTextStyle tigStyle,
            ReadOnlySpan<char> text)
        {
            var result = new FormattedText();
            result.defaultStyle = defaultStyle;
            var textBuilder = new StringBuilder(text.Length);

            bool inColorRange = false;
            bool inEscape = false;
            foreach (var ch in text)
            {
                if (ch == '@')
                {
                    inEscape = true;
                }
                else if (inEscape)
                {
                    inEscape = false;

                    if (ch == 't')
                    {
                        textBuilder[textBuilder.Length - 1] = '\t';
                        continue;
                    }
                    else if (char.IsDigit(ch))
                    {
                        var colorIdx = ch - '0';

                        // Remove the @ that we're about to remove from the previous color range
                        if (inColorRange)
                        {
                            var tmp = result.Formats[result.Formats.Count - 1];
                            tmp.length--;
                            result.Formats[result.Formats.Count - 1] = tmp;
                        }

                        // Remove last CHAR (@)
                        textBuilder.Length--;

                        if (colorIdx == 0 || !tigStyle.textColor.HasValue)
                        {
                            // Return to the normal formatting
                            inColorRange = false;
                        }
                        else
                        {
                            inColorRange = true;

                            // Add a constrainted text style
                            ConstrainedTextStyle newStyle = new ConstrainedTextStyle(defaultStyle);
                            newStyle.startChar = textBuilder.Length;

                            // Set the desired color
                            newStyle.style.foreground.gradient = false;
                            newStyle.style.foreground.primaryColor = tigStyle.GetTextColor(colorIdx).topLeft;

                            result.Formats.Add(newStyle);
                        }

                        continue;
                    }
                }

                if (inColorRange)
                {
                    // Extend the colored range by one CHAR
                    var tmp = result.Formats[result.Formats.Count - 1];
                    tmp.length++;
                    result.Formats[result.Formats.Count - 1] = tmp;
                }

                textBuilder.Append(ch);
            }

            result.text = textBuilder.ToString();
            return result;
        }

        private const string sEllipsis = "...";
        private TextEngine mTextEngine;
        private FontRenderer mRenderer;
        private FontsMapping mMapping;
        private ShapeRenderer2d mShapeRenderer;

    }
}