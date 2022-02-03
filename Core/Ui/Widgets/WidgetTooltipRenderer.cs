#nullable enable
using System.Collections.Immutable;
using System.Drawing;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetTooltipRenderer
{
    private string _tooltipStyleId = "default-tooltip";
    private InlineElement? _tooltipContent;
    private WidgetText? _tooltipLabel;

    public bool AlignLeft { get; set; }

    public string TooltipStyle
    {
        get => _tooltipStyleId;
        set
        {
            _tooltipStyleId = value;
            UpdateTooltipLabel();
        }
    }

    public string? TooltipText
    {
        get => _tooltipContent?.TextContent;
        set
        {
            if (value == null)
            {
                _tooltipContent = null;
            }
            else
            {
                _tooltipContent = new SimpleInlineElement(value);
            }
            UpdateTooltipLabel();
        }
    }

    public InlineElement? TooltipContent
    {
        get => _tooltipContent;
        set
        {
            _tooltipContent = value;
            UpdateTooltipLabel();
        }
    }

    private void UpdateTooltipLabel()
    {
        if (_tooltipContent == null)
        {
            _tooltipLabel = null;
            return;
        }

        if (_tooltipLabel == null)
        {
            _tooltipLabel = new WidgetText(_tooltipContent, _tooltipStyleId);
        }
        else
        {
            _tooltipLabel.StyleIds = ImmutableList.Create(_tooltipStyleId);
            _tooltipLabel.Content = _tooltipContent;
        }
    }

    public void Render(int x, int y)
    {
        if (_tooltipLabel != null)
        {
            // Pre-seed the max width for the content
            _tooltipLabel.SetBounds(new Rectangle(0, 0, 300, 300));
            _tooltipLabel.InvalidateStyles();

            var preferredSize = _tooltipLabel.GetPreferredSize();
            var contentArea = new Rectangle(
                x,
                y - preferredSize.Height,
                preferredSize.Width,
                preferredSize.Height
            );
            if (AlignLeft)
            {
                contentArea.X -= preferredSize.Width;
            }
            UiSystems.Tooltip.ClampTooltipToScreen(ref contentArea);
            _tooltipLabel.SetBounds(contentArea);
            _tooltipLabel.Render();
        }
    }
}