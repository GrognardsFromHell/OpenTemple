using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet;

public class CharUiMainWidget : WidgetContainer
{
    private readonly WidgetText _attributeRollCountLabel;
    private readonly WidgetText _attributeRollModeLabel;
    private readonly WidgetImage _ironmanBackground;
    private readonly WidgetImage _normalBackground;
    private readonly string _pointBuyLabel;

    public CharUiMainWidget()
    {
        PixelSize = new SizeF(658, 447);
        
        _normalBackground = new WidgetImage("art/interface/char_ui/main_window.img");
        _ironmanBackground = new WidgetImage("art/interface/char_ui/ironman_main_window.img");
        PixelSize = _normalBackground.GetPreferredSize();

        var pcCreationMes = Tig.FS.ReadMesFile("mes/pc_creation.mes");

        var rerollsLabel = pcCreationMes[10001];
        _pointBuyLabel = pcCreationMes[10015];

        _attributeRollModeLabel = new WidgetText(rerollsLabel, "char-ui-attribute-mode");
        _attributeRollCountLabel = new WidgetText("", "char-ui-attribute-rerolls");
    }

    private GameObject CurrentCritter => UiSystems.CharSheet.CurrentCritter;

    [TempleDllLocation(0x101445f0)]
    public override void Render(UiRenderContext context)
    {
        ClearContent();
        if (Globals.GameLib.IsIronmanGame)
        {
            AddContent(_ironmanBackground);
        }
        else
        {
            AddContent(_normalBackground);
        }

        if (CurrentCritter != null && CurrentCritter.IsPC())
        {
            AddRollCountLabel();
        }

        base.Render(context);
    }

    private void AddRollCountLabel()
    {
        var rollCount = CurrentCritter.GetInt32(obj_f.pc_roll_count);
        string rollCountText;
        if (rollCount == -25)
        {
            rollCountText = _pointBuyLabel;
        }
        else if (rollCount < 1 || Globals.GameLib.IsIronmanGame)
        {
            rollCountText = "-";
        }
        else
        {
            rollCountText = rollCount.ToString(CultureInfo.InvariantCulture);
        }

        _attributeRollCountLabel.Text = rollCountText;
        _attributeRollCountLabel.Y = 11;
        // Right-align
        _attributeRollCountLabel.X = LayoutBox.Width - _attributeRollCountLabel.GetPreferredSize().Width - 28;

        _attributeRollModeLabel.Y = 11;
        // Right-align
        _attributeRollModeLabel.X = _attributeRollCountLabel.X - _attributeRollModeLabel.GetPreferredSize().Width - 2;

        AddContent(_attributeRollCountLabel);
        AddContent(_attributeRollModeLabel);
    }
}