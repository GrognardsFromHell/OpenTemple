using System.Text;
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
        internal override void Format(StringBuilder builder)
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
    }
}