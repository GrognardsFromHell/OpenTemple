using System;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // Skill attempt, formerly type 2
    public class HistorySkillCheck : HistoryEntry
    {
        public Dice dice;
        public int rollResult;
        public SkillId skillIdx;
        public int dc;
        public BonusList bonlist;

        public override string Title => GameSystems.RollHistory.GetTranslation(59); // Skill Check

        [TempleDllLocation(0x10048560)]
        public override void FormatShort(StringBuilder builder)
        {
            AppendAttempt(builder);
            var effectiveRoll = rollResult + bonlist.OverallBonus;
            AppendSuccessOrFailureWithLink(builder, effectiveRoll >= dc);
            builder.Append('\n');
        }

        [TempleDllLocation(0x1019bf10)]
        public override void FormatLong(StringBuilder builder)
        {
            AppendHeader(builder);

            builder.Append(GameSystems.RollHistory.GetTranslation(32)); // Bonus
            builder.Append('\n');

            bonlist.FormatTo(builder);

            var overallBonus = bonlist.OverallBonus;
            AppendOverallBonus(builder, overallBonus);

            AppendOutcome(builder, overallBonus);
        }

        private void AppendHeader(StringBuilder builder)
        {
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(22)); // performs
            builder.Append(' ');
            builder.Append(GameSystems.Skill.GetSkillName(skillIdx));
            if (obj2 != null)
            {
                builder.Append(' ');
                builder.Append(GameSystems.RollHistory.GetTranslation(23)); // on
                builder.Append(' ');
                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
            }

            builder.Append('\n');
        }

        private static void AppendOverallBonus(StringBuilder builder, int overallBonus)
        {
            if (overallBonus >= 0)
            {
                builder.Append('+');
            }

            builder.Append(overallBonus);
            builder.Append("    ");
            builder.Append(GameSystems.RollHistory.GetTranslation(2)); // Total
            builder.Append('\n');
        }

        private void AppendOutcome(StringBuilder builder, int overallBonus)
        {
            var effectiveRoll = overallBonus + rollResult;

            builder.Append(GameSystems.RollHistory.GetTranslation(5));
            builder.Append(' ');
            builder.Append(rollResult);
            builder.Append(" + ");
            builder.Append(overallBonus);
            builder.Append(" = ");
            builder.Append(effectiveRoll);
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(24)); // Vs
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(25)); // DC
            builder.Append(' ');
            builder.Append(dc);
            builder.Append(' ');
            if (effectiveRoll < dc)
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(21)); // Failure
            }
            else
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(20)); // Success
            }

            builder.Append('\n');
        }

        private void AppendAttempt(StringBuilder builder)
        {
            var objName = GameSystems.MapObject.GetDisplayNameForParty(obj);
            var translation = GameSystems.RollHistory.GetTranslation(17); // attempts
            var skillName = GameSystems.Skill.GetSkillName(skillIdx);
            builder.AppendFormat("{0} {1} {2}", objName, translation, skillName);
        }
    }
}