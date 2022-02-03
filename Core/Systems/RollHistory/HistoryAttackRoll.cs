using System;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Systems.RollHistory
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
        public override void FormatShort(StringBuilder builder)
        {
            var rerollSuffix = "";
            if ((d20Caf & D20CAF.REROLL) != 0)
            {
                rerollSuffix = GameSystems.RollHistory.GetTranslation(4);
            }

            if (critRollRes > 0 && (d20Caf & D20CAF.CRITICAL) != 0)
            {
                AppendLine(builder, 34);
                builder.Append('!');
                builder.Append(rerollSuffix);
            }
            else
            {
                if ((d20Caf & D20CAF.HIT) != 0 || (d20Caf & D20CAF.DEFLECT_ARROWS) != 0)
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

        [TempleDllLocation(0x1019b840)]
        public override void FormatLong(StringBuilder builder)
        {
            AppendHeader(builder);

            AppendAttackBonus(builder, out var overallBonus);

            AppendHitOrMissResult(builder, overallBonus);

            AppendCriticalHitConfirmation(builder, overallBonus);

            AppendTouchAttackLine(builder);
        }

        private void AppendHeader(StringBuilder builder)
        {
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(0)); // attacks
            builder.Append(' ');
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
            builder.Append("...\n\n\n\n");
        }

        private void AppendAttackBonus(StringBuilder builder, out int overallBonus)
        {
            builder.Append(GameSystems.RollHistory.GetTranslation(3)); // Attack Bonus
            builder.Append('\n');

            bonlist.FormatTo(builder);

            overallBonus = bonlist.OverallBonus;
            if (overallBonus >= 0)
            {
                builder.Append('+');
            }

            builder.Append(overallBonus);
            builder.Append("   ");
            builder.Append(GameSystems.RollHistory.GetTranslation(2)); // Total
            builder.Append("\n\n\n");
        }

        private void AppendHitOrMissResult(StringBuilder builder, int overallBonus)
        {
            var effectiveResult = rollRes + overallBonus;

            if ((d20Caf & D20CAF.REROLL) != 0)
            {
                // TODO It forgot to append it.......?
                GameSystems.RollHistory.GetTranslation(4); // Reroll
            }

            if (rollRes == 1)
            {
                var criticalMiss = GameSystems.RollHistory.GetTranslation(70); // Critical Miss
                var rollText = GameSystems.RollHistory.GetTranslation(5); // Roll
                builder.Append($"{rollText} {rollRes} ({criticalMiss})");
            }
            else
            {
                var acText = GameSystems.RollHistory.GetTranslation(33); // AC
                var vsText = GameSystems.RollHistory.GetTranslation(24); // Vs
                var rollText = GameSystems.RollHistory.GetTranslation(5); // Roll
                builder.Append(
                    $"{rollText} {rollRes} + {overallBonus} = {effectiveResult} {vsText} ~{acText} {defenderOverallBonus}~[ROLL_{defenderRollId}]"
                );
            }

            builder.Append(' ');

            if ((d20Caf & D20CAF.HIT) != 0 || (d20Caf & D20CAF.DEFLECT_ARROWS) != 0)
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(6)); // Hit
                if ((d20Caf & D20CAF.DEFLECT_ARROWS) != 0)
                {
                    builder.Append(' ');
                    builder.Append(GameSystems.RollHistory.GetTranslation(8)); // Deflect Arrows
                }
            }
            else if (rollRes != 1)
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(9)); // Miss
            }

            builder.Append('\n');
        }

        private void AppendCriticalHitConfirmation(StringBuilder builder, int overallBonus)
        {
            if (critRollRes > 0)
            {
                if ((d20Caf & D20CAF.REROLL_CRITICAL) != 0)
                {
                    // TODO: And then it was ignored...
                    GameSystems.RollHistory.GetTranslation(4);
                }

                builder.Append(GameSystems.RollHistory.GetTranslation(10)); // Critical
                builder.Append(' ');
                builder.Append(critRollRes);
                builder.Append(" + ");
                builder.Append(overallBonus);
                builder.Append(" = ");
                builder.Append(critRollRes + overallBonus);
                builder.Append(' ');
                if ((d20Caf & D20CAF.CRITICAL) != 0)
                {
                    builder.Append(GameSystems.RollHistory.GetTranslation(11)); // confirmed
                }
                else
                {
                    builder.Append(GameSystems.RollHistory.GetTranslation(12)); // confirmation failed
                }

                builder.Append('\n');
            }
        }

        private void AppendTouchAttackLine(StringBuilder builder)
        {
            if ((d20Caf & D20CAF.TOUCH_ATTACK) != 0)
            {
                if ((d20Caf & D20CAF.RANGED) != 0)
                {
                    builder.Append(GameSystems.RollHistory.GetTranslation(13)); // Ranged touch attack
                }
                else
                {
                    builder.Append(GameSystems.RollHistory.GetTranslation(14)); // Touch attack
                }

                builder.Append('\n');
            }
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