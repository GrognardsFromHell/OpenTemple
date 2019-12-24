using System;
using System.Text;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.RollHistory
{
    // Formerly type 3
    public class HistorySavingThrow : HistoryEntry
    {
        public Dice dicePacked;
        public int rollResult;
        public int dc;
        public SavingThrowType saveType;
        public D20SavingThrowFlag saveFlags;
        public BonusList bonlist;

        public override string Title => GameSystems.RollHistory.GetTranslation(60); // Saving Throw

        private void AppendAttempt(StringBuilder builder)
        {
            var objName = GameSystems.MapObject.GetDisplayNameForParty(obj);
            var translation = GameSystems.RollHistory.GetTranslation(17); // attempts
            var saveTypeText = GameSystems.D20.Combat.GetSaveTypeName(saveType);
            var saveText = GameSystems.RollHistory.GetTranslation(18); // save
            builder.AppendFormat("{0} {1} {2}  {3}", objName, translation, saveTypeText, saveText);
        }

        [TempleDllLocation(0x10048670)]
        public override void FormatShort(StringBuilder builder)
        {
            AppendAttempt(builder);
            var overallBon = bonlist.OverallBonus;
            AppendSuccessOrFailureWithLink(builder, rollResult + overallBon >= dc);
            builder.Append('\n');
        }

        [TempleDllLocation(0x1019c210)]
        public override void FormatLong(StringBuilder builder)
        {
            AppendHeader(builder);

            builder.Append(GameSystems.RollHistory.GetTranslation(32)); // Bonus
            builder.Append("\n");
            bonlist.FormatTo(builder);
            builder.Append("\n\n");

            int overallBonus = bonlist.OverallBonus;
            AppendOverallBonus(builder, overallBonus);

            AppendOutcome(builder, overallBonus);
        }

        private void AppendHeader(StringBuilder builder)
        {
            builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(26)); // attempts a
            builder.Append(' ');
            builder.Append(GameSystems.D20.Combat.GetSaveTypeName(saveType));
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(18)); // save
            builder.Append("\n\n\n\n");
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
            builder.Append("\n\n\n");
        }

        private void AppendOutcome(StringBuilder builder, int overallBonus)
        {
            builder.Append(GameSystems.RollHistory.GetTranslation(5)); // Roll

            var effectiveRoll = overallBonus + rollResult;
            if ((saveFlags & D20SavingThrowFlag.REROLL) != 0)
            {
                builder.Append(GameSystems.RollHistory.GetTranslation(4)); // Reroll
            }

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

    }
}