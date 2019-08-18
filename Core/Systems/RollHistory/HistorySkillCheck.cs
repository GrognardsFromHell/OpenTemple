using System;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // Skill attempt
    public class HistorySkillCheck : HistoryEntry
    {
        public Dice dicePacked;
        public int rollResult;
        public SkillId skillIdx;
        public int dc;
        public BonusList bonlist;

        [TempleDllLocation(0x10048560)]
        internal override void PrintToConsole(StringBuilder builder)
        {
            AppendAttempt(builder);
            var effectiveRoll = rollResult + bonlist.OverallBonus;
            AppendSuccessOrFailureWithLink(builder, effectiveRoll >= dc);
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