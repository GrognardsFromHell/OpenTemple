using System;
using System.Drawing;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Stats;

public class StatsLabel : WidgetButtonBase
{
    private readonly WidgetImage _hoverImage;

    private readonly WidgetImage _downImage;

    private readonly WidgetText _label;

    private readonly Stat _stat;

    public StatsLabel(
        Stat stat,
        string? helpTopic,
        Rectangle rect,
        StatsUiTexture downImage,
        StatsUiTexture hoverImage,
        StatsUiParams uiParams) : base(rect)
    {
        _stat = stat;
        _downImage = new WidgetImage(uiParams.TexturePaths[downImage]);
        _hoverImage = new WidgetImage(uiParams.TexturePaths[hoverImage]);

        var statName = GetStatName(uiParams, stat);

        _label = new WidgetText(statName, "char-ui-stat-label");
        OnMouseLeave += _ => { UiSystems.CharSheet.Help.ClearHelpText(); };
        if (helpTopic != null)
        {
            AddClickListener(() => { GameSystems.Help.ShowTopic(helpTopic); });
        }
    }

    private static string GetStatName(StatsUiParams uiParams, Stat stat)
    {
        if (stat == Stat.initiative_bonus || stat == Stat.movement_speed)
        {
            return GameSystems.Stat.GetStatName(stat);
        }
        else if (stat == Stat.melee_attack_bonus)
        {
            return uiParams.PrimaryAtkLabelText;
        }
        else if (stat == Stat.ranged_attack_bonus)
        {
            return uiParams.SecondaryAtkLabelText;
        }
        else
        {
            return GameSystems.Stat.GetStatShortName(stat);
        }
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
                -1,
                _downImage.GetPreferredSize().Width,
                _downImage.GetPreferredSize().Height
            )
        );
        _hoverImage.SetBounds(
            new RectangleF(
                0,
                -1,
                _hoverImage.GetPreferredSize().Width,
                _hoverImage.GetPreferredSize().Height
            )
        );

        var labelSize = _label.GetPreferredSize();
        // Center horizontally and vertically within the content area
        var labelArea = new RectangleF(
            (PaddingArea.Width - labelSize.Width) / 2,
            (PaddingArea.Height - labelSize.Height) / 2,
            labelSize.Width,
            labelSize.Height
        );

        // Special cases for certain labels
        if (_stat == Stat.level)
        {
            labelArea.X += 2;
        }
        else if (_stat == Stat.initiative_bonus)
        {
            labelArea.X += 1;
        }

        _label.SetBounds(labelArea);
    }

    public override void Render(UiRenderContext context)
    {
        WidgetImage? renderImage = null;
        if (ContainsPress)
        {
            renderImage = _downImage;
        }
        else if (ContainsMouse)
        {
            renderImage = _hoverImage;
        }

        var paddingArea = GetViewportPaddingArea();

        renderImage?.Render(paddingArea.Location);

        _label.Render(paddingArea.Location);
    }
}