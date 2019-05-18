using System;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet
{
    public class CharUiAlignGenderRace : WidgetButton
    {
        private readonly CharUiParams _uiParams;

        public CharUiAlignGenderRace(CharUiParams uiParams)
        {
            SetPos(uiParams.CharUiMainAlignmentGenderRaceButton.Location);
            SetSize(uiParams.CharUiMainAlignmentGenderRaceButton.Size);
            _uiParams = uiParams;
        }

        [TempleDllLocation(0x10145020)]
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
                kerning = 1,
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW
            };
            Tig.Fonts.PushFont(_uiParams.FontBig, _uiParams.FontBigSize);

            var contentArea = GetContentArea();
            int maxWidth = contentArea.Width;

            var text = BuildText(currentCritter);

            var textMeas = Tig.Fonts.MeasureTextSize(text, textStyle);
            if (textMeas.Width > maxWidth)
            {
                // if still too long, truncate
                textStyle.flags |= TigTextStyleFlag.TTSF_TRUNCATE;
                textMeas = Tig.Fonts.MeasureTextSize(text, textStyle, maxWidth);
            }

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

        private string BuildText(GameObjectBody currentCritter)
        {
            var textBuilder = new StringBuilder();

            // Alignment
            if (currentCritter.IsPC() || Globals.Config.ShowNpcStats){
                var alignment = (Alignment)GameSystems.Stat.StatLevelGet(currentCritter, Stat.alignment);
                var alignmentName = GameSystems.Stat.GetAlignmentName(alignment);
                textBuilder.Append(alignmentName).Append(' ');
            }

            // Subtype
            if (currentCritter.IsNPC()){
                var isHuman = GameSystems.Critter.IsCategorySubtype(currentCritter, MonsterSubtype.human)
                    && GameSystems.Critter.IsCategoryType(currentCritter, MonsterCategory.humanoid);

                for (var i = 0; (1 << i) <= (int) MonsterSubtype.water; i += 1) {
                    var monSubcat = (MonsterSubtype)(1 << i);
                    if (monSubcat == MonsterSubtype.human && isHuman)
                    {
                        continue; // skip silly string of "Human Humanoid"
                    }

                    if (GameSystems.Critter.IsCategorySubtype(currentCritter, monSubcat))
                    {
                        textBuilder
                            .Append(GameSystems.Stat.GetMonsterSubcategoryName(i))
                            .Append(' ');
                    }
                }

            }

            var gender = GameSystems.Stat.StatLevelGet(currentCritter, Stat.gender);
            var genderName = GameSystems.Stat.GetGenderName(gender);

            if (currentCritter.IsPC()) {
                var race = GameSystems.Critter.GetRace(currentCritter, false);
                var raceName = GameSystems.Stat.GetRaceName(race);
                textBuilder.Append(genderName).Append(' ').Append(raceName);
            } else {
                var moncat = GameSystems.Critter.GetCategory(currentCritter);
                var monsterCatName = GameSystems.Stat.GetMonsterCategoryName(moncat);

                textBuilder.Append(genderName).Append(' ').Append(monsterCatName);
            }

            return textBuilder.ToString();
        }
    }
}