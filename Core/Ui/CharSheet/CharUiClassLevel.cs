using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ImGuiNET;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet
{
    public class CharUiClassLevel : WidgetButton
    {
        private readonly CharUiParams _uiParams;

        private readonly string _textSeparator;
        private readonly string _textLevel;
        private readonly string _textNpc;

        public CharUiClassLevel(CharUiParams uiParams)
        {
            SetPos(uiParams.CharUiMainClassLevelButton.Location);
            SetSize(uiParams.CharUiMainClassLevelButton.Size);
            _uiParams = uiParams;

            var translations = Tig.FS.ReadMesFile("mes/0_char_ui_text.mes");
            _textSeparator = ' ' + translations[1600] + ' ';
            _textLevel = translations[1590];
            _textNpc = '(' + translations[1610] + ')';
        }

        [TempleDllLocation(0x10144b40)]
        public override void Render()
        {
            var currentCritter = UiSystems.CharSheet.CurrentCritter;
            if (currentCritter == null)
            {
                return;
            }

            var textStyle = new TigTextStyle(new ColorRect(_uiParams.FontNormalColor))
            {
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                tracking = 2,
                kerning = 0,
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW
            };
            Tig.Fonts.PushFont(_uiParams.FontBig, _uiParams.FontBigSize);

            int maxWidth = 340;

            var text = BuildClassText(currentCritter, false);

            var textMeas = Tig.Fonts.MeasureTextSize(text, textStyle);
            if (textMeas.Width > maxWidth)
            {
                // get class shortnames
                text = BuildClassText(currentCritter, true);
            }

            textMeas = Tig.Fonts.MeasureTextSize(text, textStyle);
            if (textMeas.Width > maxWidth)
            {
                // if still too long, truncate
                textStyle.flags |= TigTextStyleFlag.TTSF_TRUNCATE;
                textMeas = Tig.Fonts.MeasureTextSize(text, textStyle, maxWidth);
            }

            var contentArea = GetContentArea();
            contentArea.X += Math.Abs(GetWidth() - textMeas.Width) / 2;
            if (textMeas.Width > 290)
            {
                contentArea.X -= 20;
            }

            contentArea.Width = textMeas.Width;
            contentArea.Height = textMeas.Height;

            Tig.Fonts.RenderText(text, contentArea, textStyle);

            Tig.Fonts.PopFont();
        }

        private string BuildClassText(GameObjectBody currentCritter, bool shortClassNames)
        {
            var textBuilder = new StringBuilder();

            if (currentCritter.IsPC() || Globals.Config.ShowNpcStats)
            {
                // cycle through classes
                bool isFirst = true;
                foreach (var classCode in D20ClassSystem.AllClasses)
                {
                    var classLvl = GameSystems.Stat.StatLevelGet(currentCritter, classCode);
                    if (classLvl <= 0)
                    {
                        continue;
                    }

                    if (!isFirst)
                    {
                        // add a "/" separator
                        textBuilder.Append(_textSeparator);
                    }
                    else
                    {
                        isFirst = false;
                    }

                    string className;
                    if (!shortClassNames)
                    {
                        className = GameSystems.Stat.GetStatName(classCode);
                    }
                    else
                    {
                        className = GameSystems.Stat.GetStatShortName(classCode);
                    }

                    textBuilder.Append($"{className} {_textLevel} {classLvl}");
                }
            }
            else
            {
                textBuilder.Append(_textNpc);
            }

            return textBuilder.ToString();
        }
    }
}