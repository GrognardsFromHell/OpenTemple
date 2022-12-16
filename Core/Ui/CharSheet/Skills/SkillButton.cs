using System;
using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Skills;

public class SkillButton : WidgetButtonBase
{
    private readonly WidgetText _skillNameLabel;

    private readonly WidgetText _skillBonusLabel;

    public SkillId Skill { get; private set; }

    public SkillButton(Rectangle rect) : base(rect)
    {
        _skillNameLabel = new WidgetText("", "char-ui-skill-button");
        _skillNameLabel.X = 4;
        _skillNameLabel.Y = -1;
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
        _skillBonusLabel.Y = _skillNameLabel.Y;
        _skillBonusLabel.FixedSize = bonusBox.FixedSize;
        _skillBonusLabel.AddStyle("char-ui-skill-value");
        AddContent(_skillBonusLabel);

        AddClickListener(ShowBonusDetails);
    }

    [TempleDllLocation(0x101bd7a0)]
    private void ShowBonusDetails()
    {
        var critter = UiSystems.CharSheet.CurrentCritter;
        var bonuses = BonusList.Create();
        critter.dispatch1ESkillLevel(Skill, ref bonuses, null, 0);

        var historyId = GameSystems.RollHistory.AddMiscBonus(critter, bonuses, 1000 + (int)Skill, 0);
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
        var totalBonus = (float)critter.dispatch1ESkillLevel(Skill, ref bonuses, null,
            SkillCheckFlags.UnderDuress);
        
        // Include the .5 ranks that are not included in the ranks
        var halfRanks = GameSystems.Skill.GetSkillHalfRanks(critter, Skill);
        if (Skill == SkillId.heal)
        {
            halfRanks++;
        }
        
        if (halfRanks == 1)
        {
            skillRanksText = "\u00BD";
        }
        else
        {
            skillRanksText = (halfRanks / 2).ToString();
            if (halfRanks % 2 != 0)
            {
                skillRanksText += "\u00BD"; // Unicode symbol for 1/2
            }
        }

        totalBonusText = totalBonus.ToString("+0;-#;0");

        // Split up the bonus list into the bonus conferred by the skill's primary attribute,
        // and group every other bonus
        var attributeBonusFound = false;
        var attributeBonusStat = GameSystems.Skill.GetDecidingStat(Skill);
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

        attributeBonusText = attributeBonusValue.ToString("+0;-#;0"); 
        attributeBonusTypeText = GameSystems.Stat.GetStatShortName(attributeBonusStat);
        miscBonusText = miscBonusValue.ToString("+0;-#;0");
    }

    [TempleDllLocation(0x101bd850)]
    public override void Render()
    {
        var hovered = ContainsMouse;
        _skillNameLabel.ToggleStyle("char-ui-skill-button-hover", hovered);
        _skillBonusLabel.ToggleStyle("char-ui-skill-button-hover", hovered);

        GetSkillBreakdown(out _, out _, out _, out _, out var totalBonus);
        _skillBonusLabel.Text = totalBonus;

        if (hovered)
        {
            var bounds = GetContentArea();
            Tig.ShapeRenderer2d.DrawRectangle(bounds, null, new PackedLinearColorA(255, 255, 255, 32));
        }

        base.Render();
    }

    public void SetSkill(SkillId skill)
    {
        Skill = skill;
        _skillNameLabel.Text = GameSystems.Skill.GetSkillName(skill);
    }
}