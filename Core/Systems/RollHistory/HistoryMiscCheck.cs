using System;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.RollHistory
{
    // Formerly type 4
    public class HistoryMiscCheck : HistoryEntry
    {
        public Dice dicePacked;
        public int rollResult;
        public int dc;
        public string text;
        public BonusList bonlist;

        public override string Title => GameSystems.RollHistory.GetTranslation(63); // Check

        private void AppendAttempt(StringBuilder builder)
        {
            var objName = GameSystems.MapObject.GetDisplayNameForParty(obj);
            var translation = GameSystems.RollHistory.GetTranslation(17); // attempts
            builder.AppendFormat("{0} {1} {2}", objName, translation, text);
        }

        [TempleDllLocation(0x100487b0)]
        public override void FormatShort(StringBuilder builder)
        {
            AppendAttempt(builder);
            var success = rollResult + bonlist.OverallBonus >= dc;
            AppendSuccessOrFailureWithLink(builder, success);
            builder.Append('\n');
        }

        [TempleDllLocation(0x1019c650)]
        public override void FormatLong(StringBuilder builder)
        {
            AppendHeader(builder);

            builder.Append(GameSystems.RollHistory.GetTranslation(32)); // Bonus
            builder.Append('\n');

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
            builder.Append(GameSystems.RollHistory.GetTranslation(26));
            builder.Append(' ');
            builder.Append(Title);
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(27));
            builder.Append("...");
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
            var effectiveRoll = rollResult + overallBonus;

            builder.Append(GameSystems.RollHistory.GetTranslation(5));
            builder.Append(' ');
            builder.Append(rollResult);
            builder.Append(" + ");
            builder.Append(overallBonus);
            builder.Append(" = ");
            builder.Append(effectiveRoll);
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(24));
            builder.Append(' ');
            builder.Append(GameSystems.RollHistory.GetTranslation(25));
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