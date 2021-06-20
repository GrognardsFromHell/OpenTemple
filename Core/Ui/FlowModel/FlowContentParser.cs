#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.FlowModel
{
    public static class FlowContentParser
    {
        public static Paragraph ParseParagraph(ReadOnlySpan<char> text)
        {
            var paragraph = new Paragraph();

            var buffer = new StringBuilder();

            void FlushBuffer()
            {
                if (buffer.Length > 0)
                {
                    paragraph.AppendContent(new SimpleInlineElement()
                    {
                        Text = buffer.ToString()
                    });
                    buffer.Clear();
                }
            }

            while (!text.IsEmpty)
            {
                if (UnicodeLinkParser.ParseLink(text, out var linkText, out var linkTarget, out var linkLength))
                {
                    FlushBuffer();
                    text = text[linkLength..];
                    var textLink = new TextLink()
                    {
                        Text = new string(linkText),
                        LinkTarget = new string(linkTarget)
                    };
                    textLink.AddStyle("link");
                    paragraph.AppendContent(textLink);
                }
                else
                {
                    buffer.Append(text[0]);
                    text = text[1..];
                }
            }
            FlushBuffer();

            return paragraph;
        }

        public static void Parse(ReadOnlySpan<char> text, List<Block> blocks)
        {
            blocks.Add(ParseParagraph(text));
        }

        public static InlineElement ParseLegacyText(ReadOnlySpan<char> text, IReadOnlyList<PackedLinearColorA> colors)
        {
            SimpleInlineElement? simpleResult = null;
            ComplexInlineElement? complexResult = null;
            StyleDefinition? bufferStyle = null;
            var buffer = new StringBuilder(text.Length);

            void FlushBuffer()
            {
                if (buffer.Length > 0)
                {
                    if (complexResult != null)
                    {
                        complexResult.AppendContent(buffer.ToString(), bufferStyle);
                    }
                    else if (simpleResult != null)
                    {
                        complexResult = new ComplexInlineElement();
                        complexResult.AppendContent(simpleResult);
                        complexResult.AppendContent(buffer.ToString(), bufferStyle);
                        simpleResult = null;
                    }
                    else
                    {
                        simpleResult = new SimpleInlineElement()
                        {
                            Text = buffer.ToString()
                        };
                    }
                    buffer.Clear();
                    bufferStyle = null;
                }
            }

            var inEscape = false;
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
                        buffer[^1] = '\t';
                        continue;
                    }
                    else if (char.IsDigit(ch))
                    {
                        var colorIdx = ch - '0';

                        // Remove last char (@)
                        buffer.Length--;

                        FlushBuffer();

                        // @0 resets back to default styling, while @1 enables the 1st additional color, and so on
                        if (colorIdx > 0 && colorIdx - 1 < colors.Count)
                        {
                            bufferStyle = new StyleDefinition()
                            {
                                Color = colors[colorIdx - 1]
                            };
                        }

                        continue;
                    }
                }

                buffer.Append(ch);
            }
            FlushBuffer();

            InlineElement? result = complexResult;
            result ??= simpleResult;
            result ??= new SimpleInlineElement()
            {
                Text = ""
            };

            return result;
        }
    }
}