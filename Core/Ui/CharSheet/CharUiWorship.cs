using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet
{
    public class CharUiWorship : WidgetButton
    {
        private readonly CharUiParams _uiParams;

        private string _labelText;

        public CharUiWorship(CharUiParams uiParams)
        {
            SetPos(uiParams.CharUiMainWorshipButton.Location);
            SetSize(uiParams.CharUiMainWorshipButton.Size);
            _uiParams = uiParams;

            var translations = Tig.FS.ReadMesFile("mes/0_char_ui_text.mes");
            _labelText = translations[1500];
        }

        [TempleDllLocation(0x10145310)]
        public override void Render()
        {
            var currentCritter = UiSystems.CharSheet.CurrentCritter;
            if (currentCritter == null)
            {
                return;
            }

            var textStyle = new TigTextStyle(new ColorRect(new PackedLinearColorA(67, 88, 110, 255)))
            {
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                tracking = 2,
                kerning = 1,
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW
            };
            Tig.Fonts.PushFont(_uiParams.FontName, _uiParams.FontSize);

            var contentArea = GetContentArea();

            // Render the "Worships: " label
            var labelSize = Tig.Fonts.MeasureTextSize(_labelText, textStyle);
            var labelBox = contentArea;
            labelBox.Width = labelSize.Width;
            labelBox.Height = labelSize.Height;
            Tig.Fonts.RenderText(_labelText, labelBox, textStyle);

            var maxWidth = contentArea.Width - labelBox.Width;
            textStyle.textColor = new ColorRect(PackedLinearColorA.White);

            var text = BuildText(currentCritter);

            var textMeas = Tig.Fonts.MeasureTextSize(text, textStyle);
            if (textMeas.Width > maxWidth)
            {
                // if still too long, truncate
                textStyle.flags |= TigTextStyleFlag.TTSF_TRUNCATE;
                textMeas = Tig.Fonts.MeasureTextSize(text, textStyle, maxWidth);
            }

            var deityBox = contentArea;
            deityBox.X += labelBox.Width;
            deityBox.Width = textMeas.Width;
            deityBox.Height = textMeas.Height;

            Tig.Fonts.RenderText(text, deityBox, textStyle);

            Tig.Fonts.PopFont();
        }

        private string BuildText(GameObjectBody currentCritter)
        {
            if (currentCritter.IsPC())
            {
                var deity = (DeityId) GameSystems.Stat.StatLevelGet(currentCritter, Stat.deity);
                return GameSystems.Deity.GetName(deity);
            }
            else
            {
                return "--";
            }
        }
    }
}