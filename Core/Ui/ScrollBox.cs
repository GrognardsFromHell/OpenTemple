using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public struct ScrollBoxSettings
{
    public Point ScrollBarPos { get; set; }

    public int ScrollBarHeight { get; set; }

    public int ScrollBarMax { get; set; }

    public RectangleF TextArea { get; set; }

    public PredefinedFont Font { get; set; }

    public int Indent { get; set; }

    public bool DontAutoScroll { get; set; }
}

public class ScrollBox : WidgetContainer
{
    private readonly TextEngine _textEngine = Tig.RenderingDevice.TextEngine;

    private readonly WidgetScrollBar _scrollbar;

    private readonly WidgetImage _background;

    private readonly FlowContentHost _contentHost = new ();

    private ScrollBoxSettings _settings;

    private bool _needsRelayout;

    /// <summary>
    /// Controls whether the widget will automatically scroll to the bottom of the view when
    /// the content changes. Used for views like the roll history or dialog log.
    /// </summary>
    public bool DontAutoScroll
    {
        get => _settings.DontAutoScroll;
        [TempleDllLocation(0x1018cca0)]
        set => _settings.DontAutoScroll = value;
    }

    private int _indent;

    public int Indent
    {
        get => _indent;
        [TempleDllLocation(0x1018ccd0)]
        set
        {
            if (_indent != value)
            {
                _contentHost.LocalStyles.Indent = value;
                _indent = value;
                InvalidateContentLayout();
            }
        }
    }

    public event Action<D20HelpLink>? OnLinkClicked;

    public string BackgroundPath
    {
        set => _background.SetTexture(value);
    }

    public void ClearLines()
    {
        _blocks.Clear();
        _laidOutParagraphs.Clear();
    }

    // The currently displayed entries
    private readonly List<Block> _blocks = new();

    private readonly List<(Paragraph, TextLayout)> _laidOutParagraphs = new();

    private float _contentHeight;

    private SimpleInlineElement? _hoveredContent;

    [TempleDllLocation(0x1018d8a0)]
    public ScrollBox(RectangleF rectangle, ScrollBoxSettings settings) : base(rectangle)
    {
        _settings = settings;

        Name = "scrollbox";
        _background = new WidgetImage();
        AddContent(_background);

        // TODO: Vanilla set the scrollBox property on the widget here, but that seems unnecessary

        var scrollBarRect = new Rectangle(settings.ScrollBarPos, new Size(13, settings.ScrollBarHeight));
        _scrollbar = new WidgetScrollBar(scrollBarRect);
        _scrollbar.SetMin(0);
        _scrollbar.Max = settings.ScrollBarMax;
        _scrollbar.SetValue(settings.ScrollBarMax);
        Add(_scrollbar);

        _contentHost.SetStyleParent(this);
        _contentHost.LocalStyles.HangingIndent = true;
        Indent = settings.Indent;
        _contentHost.OnStyleChanged += InvalidateContentLayout;
        _contentHost.OnTextFlowChanged += InvalidateContentLayout;
        
        // Forward scroll wheel messages to the scrollbar
        OnMouseWheel += e =>
        {
            _scrollbar.DispatchMouseWheel(e);
        };
        OnMouseMove += e =>
        {
            UpdateHoveredElement((int) e.X, (int) e.Y);
        };
    }

    protected void Relayout()
    {
        _laidOutParagraphs.Clear();
        _contentHeight = 0;
        var lineCount = 0;
        foreach (var block in _blocks)
        {
            if (block is Paragraph paragraph)
            {
                var textLayout = _textEngine.CreateTextLayout(paragraph, _settings.TextArea.Width, 0);

                _laidOutParagraphs.Add((paragraph, textLayout));

                _contentHeight += textLayout.OverallHeight;
                lineCount += textLayout.LineCount;
            }
        }

        var prevMax = _scrollbar.Max;
        _scrollbar.Max = Math.Max(0, (int) Math.Ceiling(_contentHeight - _settings.TextArea.Height));
        if (prevMax != _scrollbar.Max)
        {
            _scrollbar.SetValue(_settings.DontAutoScroll ? 0 : _scrollbar.Max);
            // Try scrolling in increments of the line height
            _scrollbar.Quantum = Math.Max(1, (int) Math.Round(_contentHeight / lineCount));
        }
    }

    [TempleDllLocation(0x1018d1b0)]
    [TempleDllLocation(0x1018ce70)]
    public void SetEntries(List<D20RollHistoryLine> entries)
    {
        _blocks.Clear();
        _laidOutParagraphs.Clear();
        _needsRelayout = true;
        foreach (var entry in entries)
        {
            FlowContentParser.Parse(entry.Text, _blocks);
        }

        foreach (var block in _blocks)
        {
            block.Host = _contentHost;
        }
    }

    [TempleDllLocation(0x1018d720)]
    protected override void HandleMouseUp(MouseEvent e)
    {
        HandleClickOnText((int) e.X, (int) e.Y);
    }

    private SimpleInlineElement? HitTestText(float x, float y)
    {
        UpdateLayoutIfNeeded();

        var textContentRect = GetTextContentRect();
        if (!textContentRect.Contains(x, y))
        {
            return null;
        }

        x -= textContentRect.Left;
        y -= textContentRect.Top - _scrollbar.GetValue();

        float currentY = 0;
        foreach (var (paragraph, textLayout) in _laidOutParagraphs)
        {
            // TODO Only consider paragraphs that actually intersect the rectangle
            if (textLayout.TryHitTest(x, y - currentY, out var start, out _))
            {
                return paragraph.GetSourceElementAt(start);
            }

            currentY += textLayout.OverallHeight;
            if (currentY >= textContentRect.Height)
            {
                break;
            }
        }

        return null;
    }

    private void UpdateHoveredElement(int x, int y)
    {
        var element = HitTestText(x, y);
        if (_hoveredContent != element)
        {
            if (_hoveredContent != null)
            {
                foreach (var ancestor in _hoveredContent.EnumerateAncestors())
                {
                    ancestor.ToggleStylingState(StylingState.Hover, false);
                }
            }

            if (element != null)
            {
                foreach (var ancestor in element.EnumerateAncestors())
                {
                    ancestor.ToggleStylingState(StylingState.Hover, true);
                }
            }

            _hoveredContent = element;
        }
    }

    private void HandleClickOnText(int x, int y)
    {
        var link = HitTestText(x, y)?.Closest<TextLink>();
        if (link != null && TryGetHelpLink(link, out var helpLink))
        {
            if (OnLinkClicked != null)
            {
                OnLinkClicked(helpLink);
            }
            else
            {
                GameSystems.Help.OpenLink(helpLink);
            }
        }
    }

    private bool TryGetHelpLink(TextLink link, out D20HelpLink helpLink)
    {
        if (link.LinkTarget == null)
        {
            helpLink = default;
            return false;
        }

        return GameSystems.Help.TryParseLink("Dynamically Generated", link.LinkTarget, out helpLink);
    }

    [TempleDllLocation(0x1018d840)]
    [TempleDllLocation(0x1018d4d0)]
    public override void Render(UiRenderContext context)
    {
        base.Render(context);

        RenderLines();
    }

    private RectangleF GetTextContentRect()
    {
        var actualTextRect = _settings.TextArea;
        var contentArea = GetViewportPaddingArea(true);
        actualTextRect.Offset(contentArea.Location);
        actualTextRect.Intersect(contentArea);
        return actualTextRect;
    }

    private void RenderLines()
    {
        UpdateLayoutIfNeeded();

        var textContentRect = GetTextContentRect();

        _textEngine.SetScissorRect(
            textContentRect.Left,
            textContentRect.Top,
            textContentRect.Width,
            textContentRect.Height
        );
        _textEngine.BeginBatch();

        try
        {
            float currentFlowY = 0;
            foreach (var (_, textLayout) in _laidOutParagraphs)
            {
                _textEngine.RenderTextLayout(
                    textContentRect.Left,
                    textContentRect.Top + currentFlowY - _scrollbar.GetValue(),
                    textLayout
                );

                currentFlowY += textLayout.OverallHeight;
            }
        }
        finally
        {
            _textEngine.EndBatch();
        }

        _textEngine.ResetScissorRect();
    }

    private void InvalidateContentLayout()
    {
        _needsRelayout = true;
        _laidOutParagraphs.Clear();
    }

    private void UpdateLayoutIfNeeded()
    {
        if (_needsRelayout)
        {
            Relayout();
            _needsRelayout = false;
        }
    }
}