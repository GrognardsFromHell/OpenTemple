using System.Collections.Generic;
using System.Globalization;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet
{
    public class CharUiMainWidget : WidgetContainer
    {
        private readonly WidgetLegacyText _attributeRollCountLabel;

        private readonly WidgetLegacyText _attributeRollModeLabel;
        private readonly WidgetImage _ironmanBackground;
        private readonly WidgetImage _normalBackground;
        private readonly string _pointBuyLabel;

        private Dictionary<int, string> _translation;

        public CharUiMainWidget(CharUiParams uiParams) : base(uiParams.CharUiMainWindow)
        {
            _normalBackground = new WidgetImage("art/interface/char_ui/main_window.img");
            _ironmanBackground = new WidgetImage("art/interface/char_ui/ironman_main_window.img");
            SetSize(_normalBackground.GetPreferredSize());

            _translation = Tig.FS.ReadMesFile("mes/0_char_ui_text.mes");
            var pcCreationMes = Tig.FS.ReadMesFile("mes/pc_creation.mes");

            var rerollsLabel = pcCreationMes[10001];
            _pointBuyLabel = pcCreationMes[10015];

            var attributeModeStyle = new TigTextStyle(new ColorRect(new PackedLinearColorA(0xFF5A7390)));
            attributeModeStyle.kerning = 1;
            attributeModeStyle.tracking = 3;

            var attributeCountStyle = attributeModeStyle.Copy();
            attributeCountStyle.textColor = new ColorRect(PackedLinearColorA.White);

            _attributeRollModeLabel = new WidgetLegacyText(rerollsLabel, PredefinedFont.PRIORY_12, attributeModeStyle);
            _attributeRollCountLabel = new WidgetLegacyText("", PredefinedFont.PRIORY_12, attributeCountStyle);
        }

        private GameObjectBody CurrentCritter => UiSystems.CharSheet.CurrentCritter;

        [TempleDllLocation(0x101445f0)]
        public override void Render()
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

            base.Render();
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
            _attributeRollCountLabel.SetY(11);
            // Right-align
            _attributeRollCountLabel.SetX(
                GetContentArea().Width - _attributeRollCountLabel.GetPreferredSize().Width - 28
            );

            _attributeRollModeLabel.SetY(11);
            // Right-align
            _attributeRollModeLabel.SetX(
                _attributeRollCountLabel.GetX() - _attributeRollModeLabel.GetPreferredSize().Width - 2
            );

            AddContent(_attributeRollCountLabel);
            AddContent(_attributeRollModeLabel);
        }
    }
}