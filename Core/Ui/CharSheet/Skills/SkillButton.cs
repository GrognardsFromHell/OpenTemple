using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Skills
{
    public class SkillButton : WidgetButtonBase
    {
        private const PredefinedFont LabelFont = PredefinedFont.ARIAL_10;

        private static readonly TigTextStyle LabelNormalStyle =
            new TigTextStyle(new ColorRect(PackedLinearColorA.White))
            {
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                kerning = 2,
                leading = 2
            };

        private static readonly TigTextStyle LabelHoverStyle =
            new TigTextStyle(new ColorRect(new PackedLinearColorA(0xFF0D6BE3)))
            {
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                kerning = 2,
                leading = 2
            };

        private static readonly TigTextStyle ValueNormalStyle;

        private static readonly TigTextStyle ValueHoverStyle;

        private WidgetLegacyText _skillNameLabel;

        private WidgetLegacyText _skillBonusLabel;

        private SkillId _skill;

        public SkillId Skill => _skill;

        static SkillButton()
        {
            ValueNormalStyle = LabelNormalStyle.Copy();
            ValueNormalStyle.flags |= TigTextStyleFlag.TTSF_CENTER;
            ValueHoverStyle = LabelHoverStyle.Copy();
            ValueHoverStyle.flags |= TigTextStyleFlag.TTSF_CENTER;
        }

        public SkillButton(Rectangle rect) : base(rect)
        {
            _skillNameLabel = new WidgetLegacyText("", LabelFont, LabelNormalStyle);
            _skillNameLabel.SetX(4);
            _skillNameLabel.SetY(1);
            AddContent(_skillNameLabel);

            // Rectangle surrounding the bonus
            var bonusBox = new WidgetRectangle();
            bonusBox.Pen = new PackedLinearColorA(0xFF80A0C0);
            bonusBox.SetX(rect.Width - 38);
            bonusBox.SetY(1);
            bonusBox.FixedSize = new Size(30, rect.Height - 2);
            AddContent(bonusBox);

            _skillBonusLabel = new WidgetLegacyText("", LabelFont, ValueNormalStyle);
            _skillBonusLabel.SetX(bonusBox.GetX());
            _skillBonusLabel.SetY(bonusBox.GetY());
            _skillBonusLabel.FixedSize = bonusBox.FixedSize;
            AddContent(_skillBonusLabel);

            SetClickHandler(ShowBonusDetails);
        }

        [TempleDllLocation(0x101bd7a0)]
        private void ShowBonusDetails()
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            var bonlist = BonusList.Create();
            critter.dispatch1ESkillLevel(_skill, ref bonlist, null, 0);

            var historyId = GameSystems.RollHistory.AddMiscBonus(critter, bonlist, 1000 + (int) _skill, 0);
            GameSystems.Help.ShowRoll(historyId);
        }

        public void GetSkillBreakdown(
            out string skillRanksText,
            out string attributeBonusText,
            out string attributeBonusTypeText,
            out string miscBonusText,
            out string totalBonusText)
        {
            var critter = UiSystems.CharSheet.CurrentCritter;

            var bonuses = BonusList.Default;
            var totalBonus = (float) critter.dispatch1ESkillLevel(_skill, ref bonuses, null,
                SkillCheckFlags.UnderDuress);
            // Include the .5 ranks that are not included in the overall total
            var halfRanks = GameSystems.Skill.GetSkillHalfRanks(critter, _skill);
            if (halfRanks % 2 != 0)
            {
                totalBonus += 0.5f;
            }

            skillRanksText = string.Format(CultureInfo.InvariantCulture, "{0:F1}", halfRanks / 2.0f);
            totalBonusText = string.Format(CultureInfo.InvariantCulture, "{0:F1}", totalBonus);

            // Split up the bonus list into the bonus confered by the skill's primary attribute,
            // and group every other bonus
            var attributeBonusFound = false;
            var attributeBonusStat = GameSystems.Skill.GetDecidingStat(_skill);
            var attributeBonusType = 2 + (int) attributeBonusStat;
            var attributeBonusValue = 0;
            var miscBonusValue = 0;
            for (var i = 0; i < bonuses.bonCount; i++)
            {
                var bonusEntry = bonuses.bonusEntries[i];
                if (!attributeBonusFound && bonusEntry.bonType == attributeBonusType)
                {
                    attributeBonusValue = bonusEntry.bonValue;
                    attributeBonusFound = true;
                }
                /* TODO Check if this is sensible or if "initial value" (=skill ranks) can be deduced via bonus type=0) */
                else if (i != 0)
                {
                    miscBonusValue += bonusEntry.bonValue;
                }
            }

            attributeBonusText = attributeBonusValue.ToString(CultureInfo.InvariantCulture);
            attributeBonusTypeText = GameSystems.Stat.GetStatShortName(attributeBonusStat);
            miscBonusText = miscBonusValue.ToString(CultureInfo.InvariantCulture);
        }

        [TempleDllLocation(0x101bd850)]
        public override void Render()
        {
            if (ButtonState == LgcyButtonState.Hovered || ButtonState == LgcyButtonState.Down)
            {
                _skillNameLabel.TextStyle = LabelHoverStyle;
                _skillBonusLabel.TextStyle = ValueHoverStyle;
            }
            else
            {
                _skillNameLabel.TextStyle = LabelNormalStyle;
                _skillBonusLabel.TextStyle = ValueNormalStyle;
            }

            GetSkillBreakdown(out _, out _, out _, out _, out var totalBonus);
            _skillBonusLabel.Text = totalBonus;

            base.Render();
        }

        public void SetSkill(SkillId skill)
        {
            _skill = skill;
            _skillNameLabel.Text = GameSystems.Skill.GetSkillName(skill);
        }
    }
}