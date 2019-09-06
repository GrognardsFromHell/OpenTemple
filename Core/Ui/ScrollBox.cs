using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Systems.Help;
using SpicyTemple.Core.Systems.RollHistory;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui
{
    public struct ScrollBoxSettings
    {

        public Point ScrollBarPos { get; set; }

        public int ScrollBarHeight { get; set; }

        public int ScrollBarMax { get; set; }

        public Rectangle TextArea { get; set; }

        public PredefinedFont Font { get; set; }

        public int Indent { get; set; }

        public bool DontAutoScroll { get; set; }
    }

    public class ScrollBox : WidgetContainer
    {
        private WidgetScrollBar _scrollbar;

        private WidgetImage _background;

        private ScrollBoxSettings _settings;

        public string BackgroundPath
        {
            set => _background.SetTexture(value);
        }

        private readonly struct ScrollBoxLine
        {
            public readonly bool Indented;

            public readonly string Text;

            public readonly List<D20HelpLink> HelpLinks;

            public ScrollBoxLine(bool indented, string text, List<D20HelpLink> helpLinks)
            {
                Indented = indented;
                Text = text;
                HelpLinks = helpLinks;
            }
        }

        private List<ScrollBoxLine> _lines;

        private int _linesVisible;

        private int AvailableWidth => GetWidth();

        private TigTextStyle _textStyle => new TigTextStyle
        {
            flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
            bgColor = new ColorRect(new PackedLinearColorA(0x99111111)),
            shadowColor = new ColorRect(PackedLinearColorA.Black),
            textColor = new ColorRect(PackedLinearColorA.White),
            additionalTextColors = new[]
            {
                new ColorRect(new PackedLinearColorA(0xFF01BEFD)),
                new ColorRect(PackedLinearColorA.White)
            },
            kerning = 1,
            tracking = 3,
            field4c = _settings.Indent
        };

        [TempleDllLocation(0x1018cdc0)]
        public void StartFontRendering(out TigFontMetrics metrics, out TigTextStyle textStyle)
        {
            Tig.Fonts.PushFont(_settings.Font);
            textStyle = _textStyle;
            metrics = new TigFontMetrics();
            metrics.width = GetWidth();
            metrics.height = 0;
        }

        public void EndFontRendering()
        {
            Tig.Fonts.PopFont();
        }

        // Find the links that are actually in this line, and adjust their start position according to our line splits
        private void FindLinks(List<D20HelpLink> originalLinks, List<D20HelpLink> adjustedLinks, int start, int length)
        {
            foreach (var link in originalLinks)
            {
                var adjustedStart = link.StartPos - start;
                if (adjustedStart < start + length && adjustedStart + length > 0)
                {
                    var adjustedLink = link;
                    adjustedLink.StartPos = adjustedStart;
                    adjustedLinks.Add(adjustedLink);
                }
            }
        }

        [TempleDllLocation(0x1018d1b0)]
        public void SetEntries(List<D20RollHistoryLine> entries)
        {
            var v24 = -1;
            var v27 = false;

            var scrollbarY = _scrollbar.GetScrollOffsetY();
            var v20 = (float) scrollbarY;
            if ((_lines.Count - _linesVisible) - 0.5f < v20)
            {
                v27 = true;
            }

            var v25 = (int) v20;
            int ringBufIdx = 0;
            StartFontRendering(out var metrics, out var textStyle);
            Tig.Fonts.Measure(textStyle, "TEST", ref metrics);
            Rectangle textAreaRect = _settings.TextArea;

            var links = new List<D20HelpLink>();

            foreach (var entry in entries)
            {
                // Each entry can also be split into multiple lines
                var entryLineOffset = 0; // This is needed to adjust help links
                var entryLines = entry.Text.Split('\n');
                foreach (var entryLine in entryLines)
                {
                    // Figure out line wraps
                    var currentColor = 0;
                    var charsConsumed = 0;
                    ReadOnlySpan<char> lineRemaining = entryLine;
                    var indented = false;

                    while (charsConsumed < entryLine.Length)
                    {
                        var currentRect = textAreaRect;
                        if (indented)
                        {
                            currentRect.Width -= _settings.Indent;
                        }

                        var charsInLine = Tig.Fonts.MeasureWordWrap(textStyle, lineRemaining, textAreaRect);

                        links.Clear();
                        FindLinks(entry.Links, links, entryLineOffset, charsInLine);

                        string lineText;
                        if (charsInLine == entryLine.Length)
                        {
                            lineText = entryLine;
                        }
                        else
                        {
                            // Ensure that if we split a line due to word wrap, it continues with the right color on the next line
                            if (currentColor != 0)
                            {
                                lineText = "@" + currentColor + entryLine.Substring(charsConsumed, charsInLine);
                            }
                            else
                            {
                                lineText = entryLine.Substring(charsConsumed, charsInLine);
                            }
                        }

                        _lines.Add(new ScrollBoxLine(indented, lineText, links));

                        // Remember what the last color of the line is so it can be continued on the next line
                        var lastAtSign = lineText.LastIndexOf('@');
                        if (lastAtSign != -1
                            && lastAtSign + 1 < lineText.Length
                            && int.TryParse(lineRemaining.Slice(lastAtSign + 1, 1), out var lastColor))
                        {
                            currentColor = lastColor;
                        }


                        entryLineOffset += charsInLine;
                        charsConsumed += charsInLine;
                        lineRemaining = lineRemaining.Slice(charsInLine);
                        indented = true; // Indent any of the following lines
                    }

                    entryLineOffset += 1; // 1 to account for the linebreak character
                }
            }

            int v12 = _settings.TextArea.Height;
            var line_count = _lines.Count;
            var visible_lines = v12 / metrics.height + 1;
            float hidden_lines = 0.0f;
            if (v27)
            {
                hidden_lines = (float) (line_count - visible_lines);
                if (hidden_lines < 0.0f)
                {
                    hidden_lines = 0.0f;
                }
            }
            else
            {
                if (v24 < 0)
                {
                    v24 = 0;
                }

                hidden_lines = ((float) (v24 - v25) + v20);
                if (hidden_lines < 0.0f)
                {
                    hidden_lines = 0.0f;
                }

                var v17 = (float) line_count - 1.0f;
                if (hidden_lines > v17)
                {
                    hidden_lines = v17;
                }
            }

            var hidden_lines_ = line_count - visible_lines;
            if (hidden_lines_ < 0)
            {
                hidden_lines_ = 0;
            }

            _scrollbar.SetMax(hidden_lines_);
            // TODO UiScrollbarSetQuantum /*0x101f9e90*/(this.scrollbarId, 1);
            // TODO UiScrollBarSetField8C /*0x101f9ee0*/(this.scrollbarId, visible_lines);
            _linesVisible = visible_lines;
            Tig.Fonts.PopFont();
            if (_settings.DontAutoScroll)
            {
                _scrollbar.SetScrollOffsetY(0);
            }
            else
            {
                _scrollbar.SetScrollOffsetY(hidden_lines_);
            }


        }

        [TempleDllLocation(0x1018d8a0)]
        public ScrollBox(Rectangle rectangle, ScrollBoxSettings settings) : base(rectangle)
        {
            _settings = settings;

            Name = "scrollbox_main_window";
            _background = new WidgetImage();
            AddContent(_background);
            // TOD SetVisible(false) // needed???
            // TODO v9.handleMessage = (LgcyWidgetHandleMsgFn)UiScrollboxMsg/*0x1018d720*/;

            // TODO: Vanilla set the scrollBox property on the widget here, but that seems unnecessary

            var scrollBarRect = new Rectangle(settings.ScrollBarPos, new Size(13, settings.ScrollBarHeight));
            _scrollbar = new WidgetScrollBar(scrollBarRect);
            Add(_scrollbar);

            _scrollbar.SetMin(0);
            _scrollbar.SetMax(settings.ScrollBarMax);
            _scrollbar.SetScrollOffsetY(settings.ScrollBarMax);
        }

        [TempleDllLocation(0x1018d840)]
        public override void Render()
        {
            base.Render();

            // ui_scrollbox_render_prep/*0x1018d1b0*/();
            // ScrollboxText/*0x1018d4d0*/(v1, v2);
        }
    }
}