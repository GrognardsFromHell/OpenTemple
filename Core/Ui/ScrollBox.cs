using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Help;
using SpicyTemple.Core.Systems.RollHistory;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;

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

        public bool DontAutoScroll
        {
            get => _settings.DontAutoScroll;
            [TempleDllLocation(0x1018cca0)]
            set => _settings.DontAutoScroll = value;
        }

        public int Indent
        {
            get => _settings.Indent;
            [TempleDllLocation(0x1018ccd0)]
            set
            {
                _settings.Indent = value;
                UpdateSettings(_settings);
                SetEntries(_entries);
            }
        }

        public event Action<D20HelpLink> OnLinkClicked;

        public string BackgroundPath
        {
            set => _background.SetTexture(value);
        }

        private readonly struct ScrollBoxLine
        {
            public readonly bool Indented;

            public readonly string Text;

            public readonly List<D20HelpLink> HelpLinks;

            public readonly List<Rectangle> HelpLinkRectangles;

            public ScrollBoxLine(bool indented, string text, List<D20HelpLink> helpLinks,
                List<Rectangle> helpLinkRectangles)
            {
                Trace.Assert(helpLinks.Count == helpLinkRectangles.Count);
                Indented = indented;
                Text = text;
                HelpLinks = helpLinks;
                HelpLinkRectangles = helpLinkRectangles;
            }
        }

        public void ClearLines()
        {
            _entries.Clear();
            _lines.Clear();
        }

        // The currently displayed entries
        private List<D20RollHistoryLine> _entries = new List<D20RollHistoryLine>();

        private List<ScrollBoxLine> _lines = new List<ScrollBoxLine>();

        private int _linesVisible;

        private TigTextStyle _textStyle;

        private int _lineHeight;

        [TempleDllLocation(0x1018cdc0)]
        private void BeginFontRendering()
        {
            Tig.Fonts.PushFont(_settings.Font);
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
                if (adjustedStart < length && adjustedStart + link.Length > 0)
                {
                    var adjustedLink = link;
                    adjustedLink.StartPos = adjustedStart;
                    // The link starts before the line-wrap
                    if (adjustedLink.StartPos < 0)
                    {
                        adjustedLink.Length += adjustedLink.StartPos;
                        adjustedLink.StartPos = 0;
                    }

                    // The link ends beyond the line wrap
                    if (adjustedStart + link.Length > length)
                    {
                        adjustedLink.Length = length - adjustedStart;
                    }

                    adjustedLinks.Add(adjustedLink);
                }
            }
        }

        private void UpdateSettings(ScrollBoxSettings settings)
        {
            _settings = settings;

            _textStyle = new TigTextStyle
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
                tabStop = _settings.Indent
            };

            // Calculate the line height once and cache it
            Tig.Fonts.PushFont(_settings.Font);
            var metrics = new TigFontMetrics();
            Tig.Fonts.Measure(_textStyle, "TEST", ref metrics);
            _lineHeight = metrics.height;
            Tig.Fonts.PopFont();

            if (_scrollbar != null)
            {
                _scrollbar.X = settings.ScrollBarPos.X;
                _scrollbar.Y = settings.ScrollBarPos.Y;
                _scrollbar.Height = settings.ScrollBarHeight;
            }
        }

        [TempleDllLocation(0x1018d8a0)]
        public ScrollBox(Rectangle rectangle, ScrollBoxSettings settings) : base(rectangle)
        {
            UpdateSettings(settings);

            Name = "scrollbox_main_window";
            _background = new WidgetImage();
            AddContent(_background);

            // TODO: Vanilla set the scrollBox property on the widget here, but that seems unnecessary

            var scrollBarRect = new Rectangle(settings.ScrollBarPos, new Size(13, settings.ScrollBarHeight));
            _scrollbar = new WidgetScrollBar(scrollBarRect);
            Add(_scrollbar);

            _scrollbar.SetMin(0);
            _scrollbar.SetMax(settings.ScrollBarMax);
            _scrollbar.SetValue(settings.ScrollBarMax);
        }

        [TempleDllLocation(0x1018d1b0)]
        [TempleDllLocation(0x1018ce70)]
        public void SetEntries(List<D20RollHistoryLine> entries)
        {
            _lines.Clear();

            // TODO: I think this logic attempts to figure out if the scrollbar should track the latest entries
            var v24 = -1;
            var v27 = false;
            var scrollbarY = _scrollbar.GetValue();
            if ((_lines.Count - _linesVisible) - 0.5f < scrollbarY)
            {
                v27 = true;
            }

            int ringBufIdx = 0;
            BeginFontRendering();
            Rectangle textAreaRect = _settings.TextArea;

            var links = new List<D20HelpLink>();

            foreach (var entry in entries)
            {
                // Each entry can also be split into multiple lines
                var lineStart = 0;
                while (lineStart <= entry.Text.Length)
                {
                    // Search for the end of line
                    var lineEnd = entry.Text.IndexOf('\n', lineStart);
                    if (lineEnd == -1)
                    {
                        lineEnd = entry.Text.Length;
                    }

                    ReadOnlySpan<char> originalLineText = entry.Text.AsSpan(lineStart, lineEnd - lineStart);

                    // Blank line
                    if (originalLineText.IsEmpty)
                    {
                        lineStart++;
                        _lines.Add(new ScrollBoxLine(false, "", new List<D20HelpLink>(), new List<Rectangle>()));
                        continue;
                    }

                    // Figure out line wraps
                    var currentColor = 0;
                    var charsConsumed = 0;
                    ReadOnlySpan<char> lineRemaining = originalLineText;
                    var indented = false;

                    var offsetInLine = 0; // This is needed to adjust help links
                    while (charsConsumed < originalLineText.Length)
                    {
                        var currentRect = textAreaRect;
                        if (indented)
                        {
                            currentRect.Width -= _settings.Indent;
                        }

                        var charsInLine = Tig.Fonts.MeasureWordWrap(_textStyle, lineRemaining, currentRect);

                        links.Clear();
                        FindLinks(entry.Links, links, lineStart + offsetInLine, charsInLine);
                        var linkRectangles = FindLinkRectanglesInLine(links, originalLineText, indented);

                        // Ensure that if we split a line due to word wrap, it continues with the right color on the next line
                        string lineText;
                        if (currentColor != 0)
                        {
                            lineText = "@" + currentColor
                                           + new string(originalLineText.Slice(charsConsumed, charsInLine));
                        }
                        else
                        {
                            lineText = new string(originalLineText.Slice(charsConsumed, charsInLine));
                        }

                        _lines.Add(new ScrollBoxLine(indented, lineText, new List<D20HelpLink>(links), linkRectangles));

                        // Remember what the last color of the line is so it can be continued on the next line
                        var lastAtSign = lineText.LastIndexOf('@');
                        if (lastAtSign != -1
                            && lastAtSign + 1 < lineText.Length
                            && char.IsDigit(lineText[lastAtSign + 1]))
                        {
                            currentColor = lineText[lastAtSign + 1] - '0';
                        }

                        if (charsInLine == 0)
                        {
                            break; // TODO This actually is a problem with too long lines, because there is no fallback
                        }

                        offsetInLine += charsInLine;
                        charsConsumed += charsInLine;
                        lineRemaining = lineRemaining.Slice(charsInLine);
                        indented = true; // Indent any of the following lines
                    }

                    lineStart = lineEnd + 1;
                }
            }

            EndFontRendering();

            int v12 = _settings.TextArea.Height;
            var line_count = _lines.Count;
            var visible_lines = (int) (v12 / _lineHeight + 1);
            var hidden_lines = 0;
            if (v27)
            {
                hidden_lines = Math.Max(0, line_count - visible_lines);
            }
            else
            {
                hidden_lines = Math.Max(0, v24);

                if (hidden_lines > line_count - 1)
                {
                    hidden_lines = line_count - 1;
                }
            }

            var scrollMax = Math.Max(0, line_count - visible_lines);
            _scrollbar.SetMax(scrollMax);
            // TODO UiScrollbarSetQuantum /*0x101f9e90*/(this.scrollbarId, 1);
            // TODO UiScrollBarSetField8C /*0x101f9ee0*/(this.scrollbarId, visible_lines);
            _linesVisible = visible_lines;
            if (_settings.DontAutoScroll)
            {
                _scrollbar.SetValue(0);
            }
            else
            {
                _scrollbar.SetValue(hidden_lines);
            }
        }

        private List<Rectangle> FindLinkRectanglesInLine(List<D20HelpLink> links, ReadOnlySpan<char> lineText, bool indented)
        {
            var linkRectangles = new List<Rectangle>(links.Count);
            foreach (var link in links)
            {
                // Measure the pixels from the left of the line
                var metrics = new TigFontMetrics();
                Tig.Fonts.Measure(_textStyle, lineText.Slice(0, link.StartPos), ref metrics);
                var linkLeft = metrics.width;
                metrics.width = 0;
                metrics.height = 0;

                Tig.Fonts.Measure(_textStyle, lineText.Slice(0, link.StartPos + link.Length), ref metrics);
                var linkRight = metrics.width;
                var linkHeight = metrics.height;

                if (indented)
                {
                    linkLeft += _settings.Indent;
                    linkRight += _settings.Indent;
                }

                linkRectangles.Add(new Rectangle(linkLeft, 0, linkRight - linkLeft, linkHeight));
            }

            return linkRectangles;
        }

        [TempleDllLocation(0x1018d720)]
        public override bool HandleMessage(Message msg)
        {
            if (msg.type == MessageType.WIDGET)
            {
                var widgetArgs = msg.WidgetArgs;
                if (widgetArgs.widgetEventType == TigMsgWidgetEvent.MouseReleased)
                {
                    HandleClickOnText(widgetArgs.x, widgetArgs.y);
                }

                return true;
            }

            return base.HandleMessage(msg);
        }

        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            // This will forward to the children of this widget
            if (base.HandleMouseMessage(msg))
            {
                return true;
            }

            // Forward scroll wheel messages to the scrollbar
            if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
            {
                _scrollbar.HandleMouseMessage(msg);
            }

            // Ensure the scrollbox is not click-through
            return true;
        }

        private void HandleClickOnText(int x, int y)
        {
            GetVisibleLines(out var firstLineIndex, out var lineCount, out var actualTextRect);

            if (actualTextRect.Contains(x, y))
            {
                var lineClicked = (y - actualTextRect.Y) / _lineHeight;
                if (lineClicked >= 0 && lineClicked < lineCount)
                {
                    var line = _lines[firstLineIndex + lineClicked];
                    // Find a link in the area
                    var relX = x - actualTextRect.X;
                    for (var i = 0; i < line.HelpLinkRectangles.Count; i++)
                    {
                        var linkRect = line.HelpLinkRectangles[i];
                        if (relX >= linkRect.Left && relX < linkRect.Right)
                        {
                            // Found a link!
                            var link = line.HelpLinks[i];
                            if (OnLinkClicked != null)
                            {
                                OnLinkClicked(link);
                            }
                            else
                            {
                                GameSystems.Help.OpenLink(link);
                            }
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x1018d840)]
        [TempleDllLocation(0x1018d4d0)]
        public override void Render()
        {
            base.Render();

            ProcessLines();
        }

        private void GetVisibleLines(out int firstLineIdx, out int linesToRender, out Rectangle boundingRect)
        {
            var actualTextRect = _settings.TextArea;
            var contentArea = GetContentArea();
            actualTextRect.Offset(contentArea.Location);
            actualTextRect.Intersect(contentArea);

            firstLineIdx = _scrollbar.GetValue();

            linesToRender = _linesVisible;
            if (linesToRender <= _lines.Count)
            {
                var v8 = actualTextRect.Height - linesToRender * _lineHeight;
                actualTextRect.Y += v8;
                actualTextRect.Height -= v8;
            }
            else
            {
                // Align at the bottom of the box if we're auto scrolling
                if (!_settings.DontAutoScroll)
                {
                    actualTextRect.Y += actualTextRect.Height - _lineHeight * _lines.Count;
                }

                linesToRender = _lines.Count;
            }

            boundingRect = actualTextRect;
        }

        private void ProcessLines()
        {
            BeginFontRendering();

            GetVisibleLines(out var firstLineIndex, out var lineCount, out var actualTextRect);

            for (var i = 0; i < lineCount; i++)
            {
                var line = _lines[firstLineIndex + i];

                var adjustedRect = actualTextRect;
                if (line.Indented)
                {
                    adjustedRect.X += _settings.Indent;
                }

                Tig.Fonts.RenderText(line.Text, adjustedRect, _textStyle);

                actualTextRect.Y += _lineHeight;
            }

            EndFontRendering();
        }

        [TempleDllLocation(0x1018ccf0)]
        private void UiScrollboxRenderLine(ref Rectangle extents, ReadOnlySpan<char> text)
        {
/*
            if ( Tig.Fonts.HitTest(_textStyle, text, extents, _pointClicked, out var v13) == 0 )
            {

                 D20RollHistoryEntry *v6;
            int v7;
            int v8;
            HelpLink *v9;
            int v10;
            HelpMiniPacket *v11;
            int v12;
            v6 = scrollbox.d20rolls;
                v7 = v6.idx;
                v8 = v6.num2;
                v12 = v13;
                if ( v7 > v8 )
                {
                    v10 = sub_100E7070(&v6.links[v7], v6.numLinks - v7, v13);
                    if ( v10 )
                    {
                        LABEL_9:
                        v11 = scrollbox.helpMini;
                        v11.state = *(_DWORD *)v10;
                        v11.historyId = *(_DWORD *)(v10 + 4);
                        v11.text = *(string *)(v10 + 8);
                        return 1;
                    }
                    v9 = scrollbox.d20rolls.links;
                    v12 = v13;
                }
                else
                {
                    v8 -= v7;
                    v9 = &v6.links[v7];
                }
                v10 = sub_100E7070(v9, v8, v12);
                if ( v10 )
                {
                    goto LABEL_9;
                }
            }
            return false;*/
        }
    }
}