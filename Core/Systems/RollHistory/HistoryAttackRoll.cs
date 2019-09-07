using System;
using System.Text;
using SpicyTemple.Core.Systems.D20;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // Formerly type 0
    public class HistoryAttackRoll : HistoryEntry
    {
        public int rollRes;
        public int critRollRes;
        public BonusList bonlist;
        public int defenderRollId;
        public int defenderOverallBonus;
        public D20CAF d20Caf;

        public override string Title => GameSystems.RollHistory.GetTranslation(57); // Attack Roll

        [TempleDllLocation(0x10048190)]
        internal override void Format(StringBuilder builder)
        {
            var rerollSuffix = "";
            if ( (d20Caf & D20CAF.REROLL) != 0 )
            {
                rerollSuffix = GameSystems.RollHistory.GetTranslation(4);
            }

            if ( critRollRes > 0 && (d20Caf & D20CAF.CRITICAL) != 0 )
            {
                AppendLine(builder, 34);
                builder.Append('!');
                builder.Append(rerollSuffix);
            }
            else
            {
                if ( (d20Caf & D20CAF.HIT) != 0 || (d20Caf & D20CAF.DEFLECT_ARROWS) != 0 )
                {
                    AppendLine(builder, 15);
                    builder.Append('!');
                    builder.Append(rerollSuffix);
                }
                else
                {
                    AppendLine(builder, 35);
                }
            }
            builder.Append('\n');
        }

        [TempleDllLocation(0x10047800)]
        private void AppendLine(StringBuilder builder, int mesLine)
        {
            var partyLeader = GameSystems.Party.GetConsciousLeader();
            var attackerName = GameSystems.MapObject.GetDisplayName(obj, partyLeader);
            var defenderName = GameSystems.MapObject.GetDisplayName(obj2, partyLeader);

            var text = GameSystems.RollHistory.GetTranslation(mesLine);
            builder.AppendFormat("{0} ~{1}~[ROLL_{2}] {3}", attackerName, text, histId, defenderName);
        }

    }
}