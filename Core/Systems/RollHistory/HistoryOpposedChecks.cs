using System;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Systems.RollHistory;

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
    public override void FormatShort(StringBuilder builder)
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

    [TempleDllLocation(0x1019c7e0)]
    public override void FormatLong(StringBuilder builder)
    {
        AppendHeader(builder);
        builder.Append("\n\n\n\n");

        builder.Append(GameSystems.RollHistory.GetTranslation(28)); // Attacker
        builder.Append('\n');

        bonusList.FormatTo(builder);

        var overallBonus = bonusList.OverallBonus;
        AppendOverallBonus(builder, overallBonus);

        AppendOutcome(builder, overallBonus);
    }

    private void AppendHeader(StringBuilder builder)
    {
        builder.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesTitleLine));
        builder.Append(" : ");
        builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
        builder.Append(' ');
        if ((flags & 2) != 0)
        {
            builder.Append(GameSystems.RollHistory.GetTranslation(49)); // defends against
        }
        else
        {
            builder.Append(GameSystems.RollHistory.GetTranslation(50)); // against
        }

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
        builder.Append("\n\n\n");
    }

    private void AppendOutcome(StringBuilder builder, int overallBonus)
    {
        builder.Append(GameSystems.RollHistory.GetTranslation(5));
        builder.Append(' ');
        builder.Append(roll);
        builder.Append(" + ");
        builder.Append(overallBonus);
        builder.Append(" = ");
        builder.Append(roll + overallBonus);
        builder.Append(' ');
        builder.Append(GameSystems.RollHistory.GetTranslation(24)); // Vs
        builder.Append(" ~");
        builder.Append(opposingRoll + opposingBonus);
        builder.Append("~[ROLL_");
        builder.Append(opposingHistoryEntry.histId);
        builder.Append("] ");
        builder.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesResultLine));
        builder.Append('\n');
    }
}