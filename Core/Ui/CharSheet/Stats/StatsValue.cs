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
        SetWidgetMsgHandler(msg =>
        {
            if (msg.widgetEventType == TigMsgWidgetEvent.Exited)
            {
                UiSystems.CharSheet.Help.ClearHelpText();
                return true;
            }

            return false;
        });

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

    public override void Render()
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

        var contentArea = GetContentArea();

        if (renderImage != null)
        {
            renderImage.SetBounds(
                new Rectangle(
                    contentArea.X,
                    contentArea.Y - 1,
                    renderImage.GetPreferredSize().Width,
                    renderImage.GetPreferredSize().Height
                )
            );
            renderImage.Render();
        }

        var critter = UiSystems.CharSheet.CurrentCritter;
        _label.Content = critter != null ? _valueSupplier(critter) : null;

        var labelSize = _label.GetPreferredSize();
        // Center horizontally and vertically within the content area
        var labelArea = new Rectangle(
            contentArea.X + (contentArea.Width - labelSize.Width) / 2,
            contentArea.Y + (contentArea.Height - labelSize.Height) / 2,
            contentArea.Width = labelSize.Width,
            contentArea.Height = labelSize.Height
        );

        _label.SetBounds(labelArea);
        _label.Render();
    }
}