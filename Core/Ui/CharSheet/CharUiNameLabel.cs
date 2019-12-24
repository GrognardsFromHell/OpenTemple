using System;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet
{
    public class CharUiNameLabel : WidgetButtonBase
    {
        private TigTextStyle _textStyle;

        private string _fontName;

        private int _fontSize;

        public CharUiNameLabel(CharUiParams uiParams)
        {
            _textStyle = new TigTextStyle(new ColorRect(uiParams.FontNormalColor))
            {
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                tracking = 2,
                kerning = 0,
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW
            };

            _fontName = uiParams.FontBig;
            _fontSize = uiParams.FontBigSize;

            SetPos(uiParams.CharUiMainNameButton.Location);
            SetSize(uiParams.CharUiMainNameButton.Size);
        }

        public override void Render()
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            if (critter == null)
            {
                return;
            }

            var displayName = GameSystems.MapObject.GetDisplayName(critter);

            Tig.Fonts.PushFont(_fontName, _fontSize);

            _textStyle.flags &= ~TigTextStyleFlag.TTSF_TRUNCATE;
            var textSize = Tig.Fonts.MeasureTextSize(displayName, _textStyle);
            var contentArea = GetContentArea();

            if (textSize.Width > contentArea.Width)
            {
                _textStyle.flags |= TigTextStyleFlag.TTSF_TRUNCATE;

                textSize = Tig.Fonts.MeasureTextSize(displayName, _textStyle, contentArea.Width);
            }

            contentArea.X += Math.Abs(contentArea.Width - textSize.Width) / 2;
            contentArea.Width = textSize.Width;
            contentArea.Height = textSize.Height;

            Tig.Fonts.RenderText(displayName, contentArea, _textStyle);

            Tig.Fonts.PopFont();
        }
    }
}