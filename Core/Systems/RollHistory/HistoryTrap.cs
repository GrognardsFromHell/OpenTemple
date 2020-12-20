using System.Text;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Systems.RollHistory
{
    // Traps, type 8
    public class HistoryTrap : HistoryEntry
    {
        public override string Title => GameSystems.RollHistory.GetTranslation(62); // Trap

        public int attackRoll;
        public int criticalConfirmRoll;
        public BonusList attackBonus;
        public int armorClass;
        public HistoryEntry armorClassDetails;
        public D20CAF caf;

        [TempleDllLocation(0x10048320)]
        public override void FormatShort(StringBuilder builder)
        {
            if (criticalConfirmRoll > 0 && (caf & D20CAF.CRITICAL) != 0)
            {
                AppendOutcome(builder, 34); // Critically Hits
            }
            else if ((caf & (D20CAF.HIT | D20CAF.DEFLECT_ARROWS)) != 0)
            {
                AppendOutcome(builder, 15); // hits
            }
            else
            {
                AppendOutcome(builder, 35); // misses
                builder.Append('\n');
                return;
            }

            if ((caf & D20CAF.REROLL) != 0)
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(4)); // Reroll
            }

            builder.Append("!\n");
        }

        [TempleDllLocation(0x100478a0)]
        private void AppendOutcome(StringBuilder builder, int outcomeMesId)
        {
            builder.Append(GameSystems.RollHistory.GetTranslation(48)); // Trap
            builder.Append(" ~");
            builder.Append(GameSystems.RollHistory.GetTranslation(outcomeMesId));
            builder.Append("~[ROLL_");
            builder.Append(histId); // Link to details of this entry
            builder.Append("] ");
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
        }

        [TempleDllLocation(0x1019cb20)]
        public override void FormatLong(StringBuilder builder)
        {
            AppendHeader(builder);
            builder.Append('\n');

            attackBonus.FormatTo(builder);

            var overallBonus = attackBonus.OverallBonus;
            AppendOverallBonus(builder, overallBonus);
            builder.Append("\n\n\n");

            builder.Append(GameSystems.RollHistory.GetTranslation(5));
            builder.Append(' ');
            builder.Append(attackRoll);
            builder.Append(" + ");
            builder.Append(overallBonus);
            builder.Append(" = ");
            builder.Append(attackRoll + overallBonus);
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(24)); // Vs
            builder.Append(" ~");
            builder.Append(GameSystems.RollHistory.GetTranslation(33)); // AC
            builder.Append(' ');
            builder.Append(armorClass);
            builder.Append("~[ROLL_");
            builder.Append(armorClassDetails.histId);
            builder.Append("]");

            if ((caf & D20CAF.HIT) != 0)
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(6));
                if ((caf & D20CAF.DEFLECT_ARROWS) != 0)
                {
                    builder.Append(' ');
                    if ((caf & (D20CAF.HIT | D20CAF.CRITICAL)) != 0)
                    {
                        // TODO: Bit weird that the critical flag would indicate unsuccessfuly deflect arrows...?
                        builder.Append(GameSystems.RollHistory.GetTranslation(7)); // Deflect arrows unsuccessful
                    }
                    else
                    {
                        builder.Append(GameSystems.RollHistory.GetTranslation(8)); // Deflect arrows successful
                    }
                }
            }
            else
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(9));
            }

            builder.Append('\n');

            if (criticalConfirmRoll > 0)
            {
                AppendCritical(builder, overallBonus);
                builder.Append("\n");
            }

            AppendTouchAttackLine(builder);
        }

        private void AppendCritical(StringBuilder builder, int overallBonus)
        {
            if ((caf & D20CAF.REROLL_CRITICAL) != 0)
            {
                // TODO: And promptly ignored!
                GameSystems.RollHistory.GetTranslation(4); // Reroll
            }

            builder.Append(GameSystems.RollHistory.GetTranslation(10)); // Critical
            builder.Append(' ');
            builder.Append(criticalConfirmRoll);
            builder.Append(" + ");
            builder.Append(overallBonus);
            builder.Append(" = ");
            builder.Append(criticalConfirmRoll + overallBonus);
            builder.Append(' ');
            if ((caf & D20CAF.CRITICAL) != 0)
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(11)); // Confirmed
            }
            else
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(12)); // Confirmation failed
            }
        }

        private void AppendHeader(StringBuilder builder)
        {
            builder.Append(GameSystems.RollHistory.GetTranslation(48)); // Trap
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(0)); // attacks
            builder.Append(' ');
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
            builder.Append("...");
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
        }

        private void AppendTouchAttackLine(StringBuilder builder)
        {
            if ((caf & D20CAF.TOUCH_ATTACK) != 0)
            {
                if ((caf & D20CAF.RANGED) != 0)
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
    }
}