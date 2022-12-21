

using System;
using System.Drawing;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetText : WidgetContent, IFlowContentHost, IDisposable
{
    private readonly Paragraph _paragraph;

    // TODO: Dispose, preferably when it becomes invisible
    private TextLayout? _textLayout;

    private bool _preferredSizeDirty = true;

    protected TextLayout TextLayout
    {
        get
        {
            var styles = ComputedStyles;

            // It's possible the content area was not initialized yet
            var availableWidth = ContentArea.Width;
            if (availableWidth <= 0)
            {
                availableWidth = FixedWidth;
                if (availableWidth <= 0)
                {
                    availableWidth = short.MaxValue;
                }
            }

            var width = MathF.Max(0, availableWidth - styles.MarginLeft - styles.MarginRight
                                     - styles.PaddingLeft - styles.PaddingRight - styles.BorderWidth);
            var availableHeight = ContentArea.Height;
            if (availableHeight <= 0)
            {
                availableHeight = FixedHeight;
                if (availableHeight <= 0)
                {
                    availableHeight = short.MaxValue;
                }
            }
            var height = MathF.Max(0, availableHeight - styles.MarginTop - styles.MarginBottom
                                      - styles.PaddingTop - styles.PaddingBottom - styles.BorderWidth);

            _textLayout ??= Tig.RenderingDevice.TextEngine.CreateTextLayout(
                _paragraph,
                width,
                height
            );
            return _textLayout;
        }
    }

    public WidgetText()
    {
        _paragraph = new Paragraph()
        {
            Host = this
        };
    }

    public WidgetText(string text, string styleId) : this()
    {
        Text = text;
        AddStyle(styleId);
    }

    public WidgetText(InlineElement content, string styleId) : this()
    {
        Content = content;
        AddStyle(styleId);
    }

    /// <summary>
    /// Indicates whether the text in this widget is currently being trimmed because not enough space
    /// is available. Depends on setting the paragraph style that actually trims text.
    /// </summary>
    public bool IsTrimmed => TextLayout.IsTrimmed;

    public string Text
    {
        get => _paragraph.TextContent;
        set
        {
            // TODO: Process mes file placeholders
            var text = Globals.UiAssets.ApplyTranslation(value);
            Content = new SimpleInlineElement()
            {
                Text = text
            };
        }
    }

    public InlineElement? Content
    {
        set
        {
            _paragraph.ClearContent();
            if (value != null)
            {
                _paragraph.AppendContent(value);
            }

            InvalidateTextLayout();
        }
    }

    private void InvalidateTextLayout()
    {
        _textLayout?.Dispose();
        _textLayout = null;
        _preferredSizeDirty = true;
    }

    public void SetCenterVertically(bool isCentered)
    {
        LocalStyles.ParagraphAlignment = isCentered ? ParagraphAlign.Center : null;
    }

    public override void Render()
    {
        if (_paragraph.IsEmpty)
        {
            return;
        }

        var styles = ComputedStyles;
        var x = ContentArea.X + styles.MarginLeft;
        var y = ContentArea.Y + styles.MarginTop;
        var width = ContentArea.Width - styles.MarginLeft - styles.MarginRight;
        var height = ContentArea.Height - styles.MarginTop - styles.MarginBottom;

        Tig.RenderingDevice.TextEngine.RenderBackgroundAndBorder(
            x, y,
            width, height,
            styles
        );

        // Move into inner area
        x += styles.PaddingLeft + styles.BorderWidth;
        y += styles.PaddingTop + styles.BorderWidth;
        width -= styles.PaddingLeft + styles.PaddingRight + styles.BorderWidth;
        height -= styles.PaddingTop + styles.PaddingBottom + styles.BorderWidth;

        // Refresh layout if layout size changed
        var textLayout = TextLayout;
        if (Math.Abs(width - textLayout.LayoutWidth) > 0.1f ||
            Math.Abs(height - textLayout.LayoutHeight) > 0.1f)
        {
            textLayout.LayoutWidth = width;
            textLayout.LayoutHeight = height;
        }

        Tig.RenderingDevice.TextEngine.RenderTextLayout(x, y, textLayout);
    }

    public override Size GetPreferredSize()
    {
        if (_preferredSizeDirty)
        {
            PreferredSize.Width = (int) Math.Ceiling(
                TextLayout.OverallWidth + ComputedStyles.MarginLeft + ComputedStyles.MarginRight
                + ComputedStyles.PaddingLeft + ComputedStyles.PaddingRight
                + ComputedStyles.BorderWidth
            );
            PreferredSize.Height = (int) Math.Ceiling(
                TextLayout.OverallHeight + ComputedStyles.MarginTop + ComputedStyles.MarginBottom
                + ComputedStyles.PaddingTop + ComputedStyles.PaddingBottom
                + ComputedStyles.BorderWidth
            );
            _preferredSizeDirty = false;
        }

        return PreferredSize;
    }

    public ColorRect[] LegacyAdditionalTextColors { get; set; }

    protected override void OnUpdateFixedSize()
    {
        InvalidateTextLayout();
    }

    protected override void OnStylesInvalidated()
    {
        base.OnStylesInvalidated();
        // We need to ensure that the styles inherited by the paragraph are up to date
        _paragraph.InvalidateStyles();
        InvalidateTextLayout();
    }

    public void NotifyStyleChanged()
    {
    }

    public void NotifyTextFlowChanged()
    {
    }

    public void Dispose()
    {
        _textLayout?.Dispose();
    }
}