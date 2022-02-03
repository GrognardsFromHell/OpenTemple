
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog;

[DialogScript(89)]
public class GloraDialog : Glora, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 4:
                originalScript = "(pc.stat_level_get( stat_race ) == race_elf) and (pc.stat_level_get( stat_gender ) == gender_male) and (pc.stat_level_get( stat_level_wizard ) >= 1) and (not npc.has_met( pc ))";
                return (pc.GetRace() == RaceId.aquatic_elf) && (pc.GetGender() == Gender.Male) && (pc.GetStat(Stat.level_wizard) >= 1) && (!npc.HasMet(pc));
            case 7:
            case 8:
                originalScript = "game.global_flags[67] == 0";
                return !GetGlobalFlag(67);
            case 501:
            case 510:
                originalScript = "game.party_alignment == LAWFUL_GOOD";
                return PartyAlignment == Alignment.LAWFUL_GOOD;
            case 502:
            case 511:
                originalScript = "game.party_alignment == CHAOTIC_GOOD";
                return PartyAlignment == Alignment.CHAOTIC_GOOD;
            case 503:
            case 512:
                originalScript = "game.party_alignment == LAWFUL_EVIL";
                return PartyAlignment == Alignment.LAWFUL_EVIL;
            case 504:
            case 513:
                originalScript = "game.party_alignment == CHAOTIC_EVIL";
                return PartyAlignment == Alignment.CHAOTIC_EVIL;
            case 505:
            case 514:
                originalScript = "game.party_alignment == TRUE_NEUTRAL";
                return PartyAlignment == Alignment.NEUTRAL;
            case 506:
            case 515:
                originalScript = "game.party_alignment == NEUTRAL_GOOD";
                return PartyAlignment == Alignment.NEUTRAL_GOOD;
            case 507:
            case 516:
                originalScript = "game.party_alignment == NEUTRAL_EVIL";
                return PartyAlignment == Alignment.NEUTRAL_EVIL;
            case 508:
            case 517:
                originalScript = "game.party_alignment == LAWFUL_NEUTRAL";
                return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
            case 509:
            case 518:
                originalScript = "game.party_alignment == CHAOTIC_NEUTRAL";
                return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            default:
                originalScript = null;
                return;
        }
    }
    public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
    {
        switch (lineNumber)
        {
            default:
                skillChecks = default;
                return false;
        }
    }
}