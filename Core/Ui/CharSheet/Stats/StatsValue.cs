using System;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Stats;

public class StatsValue : WidgetButtonBase
{
    private readonly WidgetImage _downImage;
    private readonly WidgetImage _hoverImage;

    private readonly WidgetText _label;

    private readonly Func<GameObject, InlineElement> _valueSupplier;

    public StatsValue(
        Func<GameObject, string> valueSupplier,
        Rectangle rect,
        StatsUiTexture downImage,
        StatsUiTexture hoverImage,
        StatsUiParams uiParams) : this(obj => new SimpleInlineElement(valueSupplier(obj)),
        rect,
        downImage,
        hoverImage,
        uiParams)
    {
    }

    public StatsValue(
        Func<GameObject, InlineElement> valueSupplier,
        Rectangle rect,
        StatsUiTexture downImage,
        StatsUiTexture hoverImage,
        StatsUiParams uiParams) : base(rect)
    {
        _downImage = new WidgetImage(uiParams.TexturePaths[downImage]);
        _hoverImage = new WidgetImage(uiParams.TexturePaths[hoverImage]);
        _valueSupplier = valueSupplier;

        _label = new WidgetText("", "char-ui-stat-value");
        OnMouseLeave += _ => {
            UiSystems.CharSheet.Help.ClearHelpText();
        };
        
        TooltipText = UiSystems.Tooltip.GetString(6044);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _downImage.Dispose();
            _hoverImage.Dispose();
        }
    }

    protected override void OnAfterLayout()
    {
        base.OnAfterLayout();

        _downImage.SetBounds(
            new RectangleF(
                0,
                - 1,
                _downImage.GetPreferredSize().Width,
                _downImage.GetPreferredSize().Height
            )
        );
        _hoverImage.SetBounds(
            new RectangleF(
                0,
                - 1,
                _hoverImage.GetPreferredSize().Width,
                _hoverImage.GetPreferredSize().Height
            )
        );        
    }

    public override void Render(UiRenderContext context)
    {
        WidgetImage renderImage = null;
        if (ContainsPress)
        {
            renderImage = _downImage;
        }
        else if (ContainsMouse)
        {
            renderImage = _hoverImage;
        }

        var paddingArea = GetViewportPaddingArea(true);
        renderImage?.Render(paddingArea.Location);

        var critter = UiSystems.CharSheet.CurrentCritter;
        _label.Content = critter != null ? _valueSupplier(critter) : null;

        var labelSize = _label.GetPreferredSize();
        // Center horizontally and vertically within the content area
        var labelArea = new RectangleF(
            (paddingArea.Width - labelSize.Width) / 2,
            (paddingArea.Height - labelSize.Height) / 2,
            labelSize.Width,
            labelSize.Height
        );

        _label.SetBounds(labelArea);
        _label.Render(PaddingArea.Location);
    }
}