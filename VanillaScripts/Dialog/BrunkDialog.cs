
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

[DialogScript(168)]
public class BrunkDialog : Brunk, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 6:
            case 7:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 9";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 9;
            case 13:
            case 14:
                originalScript = "( game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD )";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
            case 15:
            case 16:
                originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL";
                return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
            case 17:
            case 18:
                originalScript = "game.party_alignment == CHAOTIC_GOOD or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL";
                return PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
            case 32:
            case 33:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 6";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
            case 42:
            case 43:
                originalScript = "game.global_flags[177] == 1";
                return GetGlobalFlag(177);
            case 53:
            case 54:
                originalScript = "pc.stat_level_get(stat_race) == race_half_orc";
                return pc.GetRace() == RaceId.half_orc;
            case 101:
            case 102:
                originalScript = "game.global_flags[178] == 0";
                return !GetGlobalFlag(178);
            case 152:
                originalScript = "pc.stat_level_get(stat_race) == race_half_orc and pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetRace() == RaceId.half_orc && pc.GetGender() == Gender.Male;
            case 153:
                originalScript = "pc.stat_level_get(stat_race) != race_half_orc and pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetRace() != RaceId.half_orc && pc.GetGender() == Gender.Male;
            case 154:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                return pc.GetGender() == Gender.Female;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1:
            case 10:
                originalScript = "game.global_flags[175] = 1";
                SetGlobalFlag(175, true);
                break;
            case 2:
            case 3:
            case 21:
            case 22:
            case 31:
            case 41:
            case 85:
            case 86:
            case 111:
            case 191:
            case 192:
            case 214:
            case 215:
            case 231:
            case 232:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 200:
                originalScript = "game.global_flags[178] = 1";
                SetGlobalFlag(178, true);
                break;
            default:
                originalScript = null;
                return;
        }
    }
    public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
    {
        switch (lineNumber)
        {
            case 6:
            case 7:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 9);
                return true;
            case 32:
            case 33:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}