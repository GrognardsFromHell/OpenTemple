using System;
using System.Text;
using Microsoft.Scripting.Generation;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // This is used for linking to a miscellaneous bonus list from other entries and such, formerly type 7
    public class HistoryBonusDetail : HistoryEntry
    {
        public BonusList BonusList { get; }

        public int MesLineId { get; }

        public int RollResult { get; }

        public override string Title => GameSystems.RollHistory.GetTranslation(64); // Bonus Detail

        public HistoryBonusDetail(BonusList bonusList, int mesLineId, int rollResult)
        {
            BonusList = bonusList;
            MesLineId = mesLineId;
            RollResult = rollResult;
        }

        public override void FormatShort(StringBuilder builder)
        {
        }

        [TempleDllLocation(0x1019c9d0)]
        public override void FormatLong(StringBuilder builder)
        {
            if (MesLineId < 1000)
            {
                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
                builder.Append("  ");
                builder.Append(GameSystems.RollHistory.GetTranslation(MesLineId));
            }
            else
            {
                var skillId = (SkillId) (MesLineId - 1000);
                var skillHelpTopic = GameSystems.Skill.GetHelpTopic(skillId);
                var skillName = GameSystems.Skill.GetSkillName(skillId);

                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
                builder.Append("  ~");
                builder.Append(skillName);
                builder.Append("~[");
                builder.Append(skillHelpTopic);
                builder.Append("]");
            }

            builder.Append("\n\n\n");

            BonusList.FormatTo(builder);
            builder.Append("\n\n");

            AppendOverallBonus(builder, BonusList.OverallBonus);
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
    }
}