using System;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // Formerly type 5
    public class HistoryPercentageCheck : HistoryEntry
    {
        public int failureChance;
        public int rollResult;
        public int combatMesFailureReason; // description for the cause of the roll (e.g.
        public int combatMesResult; // descrition for the result
        public int combatMesTitle;

        public override string Title => GameSystems.RollHistory.GetTranslation(65); // Percent Roll

        [TempleDllLocation(0x10047780)]
        public override void FormatShort(StringBuilder builder)
        {
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(17)); // attempts
            builder.Append(' ');
            builder.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesFailureReason));
            builder.AppendFormat(
                " : ~{0}~[ROLL_{1}]\n",
                GameSystems.D20.Combat.GetCombatMesLine(combatMesResult),
                histId
            );
            builder.Append(GameSystems.RollHistory.GetTranslation(24)); // "Vs"
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
        }

        [TempleDllLocation(0x1019c4f0)]
        public override void FormatLong(StringBuilder builder)
        {
            if (obj2 != null)
            {
                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
                builder.Append(' ');
                builder.Append(GameSystems.RollHistory.GetTranslation(17)); // attemptps
                builder.Append(' ');
                builder.Append(GameSystems.RollHistory.GetTranslation(combatMesFailureReason));
                builder.Append(' ');
                builder.Append(GameSystems.RollHistory.GetTranslation(24)); // Vs
                builder.Append(' ');
                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
            }
            else
            {
                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
                builder.Append(' ');
                builder.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesFailureReason));
            }

            builder.Append("\n\n\n");

            AppendOutcome(builder);
        }

        private void AppendOutcome(StringBuilder builder)
        {
            builder.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesTitle));
            builder.Append(' ');
            builder.Append(failureChance);
            builder.Append("%: ");
            builder.Append(GameSystems.RollHistory.GetTranslation(5)); // Roll
            builder.Append(' ');
            builder.Append(rollResult);
            builder.Append("- ");
            builder.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesResult));
            builder.Append('\n');
        }
    }
}