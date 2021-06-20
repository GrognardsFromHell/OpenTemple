using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Skills
{
    public class SkillButton : WidgetButtonBase
    {
        private readonly WidgetText _skillNameLabel;

        private readonly WidgetText _skillBonusLabel;

        private SkillId _skill;

        public SkillId Skill => _skill;

        public SkillButton(Rectangle rect) : base(rect)
        {
            _skillNameLabel = new WidgetText("", "char-ui-skill-button");
            _skillNameLabel.X = 4;
            _skillNameLabel.Y = 1;
            AddContent(_skillNameLabel);

            // Rectangle surrounding the bonus
            var bonusBox = new WidgetRectangle();
            bonusBox.Pen = new PackedLinearColorA(0xFF80A0C0);
            bonusBox.X = rect.Width - 38;
            bonusBox.Y = 1;
            bonusBox.FixedSize = new Size(30, rect.Height - 2);
            AddContent(bonusBox);

            _skillBonusLabel = new WidgetText("", "char-ui-skill-button");
            _skillBonusLabel.X = bonusBox.X;
            _skillBonusLabel.Y = bonusBox.Y;
            _skillBonusLabel.FixedSize = bonusBox.FixedSize;
            _skillBonusLabel.AddStyle("char-ui-skill-value");
            AddContent(_skillBonusLabel);

            SetClickHandler(ShowBonusDetails);
        }

        [TempleDllLocation(0x101bd7a0)]
        private void ShowBonusDetails()
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            var bonlist = BonusList.Create();
            critter.dispatch1ESkillLevel(_skill, ref bonlist, null, 0);

            var historyId = GameSystems.RollHistory.AddMiscBonus(critter, bonlist, 1000 + (int)_skill, 0);
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
            var totalBonus = (float)critter.dispatch1ESkillLevel(_skill, ref bonuses, null,
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
            var attributeBonusType = 2 + (int)attributeBonusStat;
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
            var hovered = ButtonState == LgcyButtonState.Hovered || ButtonState == LgcyButtonState.Down;
            _skillNameLabel.ToggleStyle("char-ui-skill-button-hover", hovered);
            _skillBonusLabel.ToggleStyle("char-ui-skill-button-hover", hovered);

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