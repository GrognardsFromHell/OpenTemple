using System.Text;
using SpicyTemple.Core.Systems.D20;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // Formerly type 6
    public class HistoryOpposedChecks : HistoryEntry
    {

        public BonusList bonusList;
        public HistoryOpposedChecks opposingHistoryEntry;
        public int roll;
        public int opposingRoll;
        public int opposingBonus;
        public int combatMesTitleLine;
        public D20CombatMessage combatMesResultLine;
        public int flags; // 2 is for the original opponent's roll

        public override string Title => GameSystems.RollHistory.GetTranslation(61); // Opposed Check

        [TempleDllLocation(0x100488b0)]
        internal override void Format(StringBuilder builder)
        {
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(17)); // attempts
            builder.Append(' ');
            builder.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesTitleLine));
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(24)); // "Vs"
            builder.Append(' ');
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
            builder.AppendFormat(
                " - ~{0}~[ROLL_{1}]",
                GameSystems.D20.Combat.GetCombatMesLine(combatMesResultLine),
                histId
            );
            builder.Append('\n');
        }
    }
}