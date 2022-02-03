
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

[DialogScript(86)]
public class AgentOfEvilDialog : AgentOfEvil, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 3:
            case 8:
                originalScript = "not npc.has_met( pc )";
                return !npc.HasMet(pc);
            case 4:
            case 5:
                originalScript = "game.quests[15].state == qs_accepted and not npc.has_met( pc ) and game.global_flags[31] == 0";
                return GetQuestState(15) == QuestState.Accepted && !npc.HasMet(pc) && !GetGlobalFlag(31);
            case 6:
            case 7:
                originalScript = "game.quests[15].state == qs_accepted and npc.has_met( pc ) and game.global_flags[31] == 0";
                return GetQuestState(15) == QuestState.Accepted && npc.HasMet(pc) && !GetGlobalFlag(31);
            case 13:
            case 14:
                originalScript = "game.global_flags[65] == 1";
                return GetGlobalFlag(65);
            case 22:
            case 31:
                originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 5";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 5;
            case 81:
            case 82:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 2";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 2;
            case 83:
            case 84:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 5";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
            case 85:
            case 86:
            case 95:
            case 96:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 7";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
            case 91:
            case 92:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 3";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 3;
            case 93:
            case 94:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 6";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
            case 105:
            case 106:
            case 115:
            case 116:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 7 and game.global_flags[65] == 1";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7 && GetGlobalFlag(65);
            case 171:
            case 211:
            case 212:
            case 222:
            case 251:
            case 252:
            case 341:
            case 342:
                originalScript = "game.global_flags[197] == 1";
                return GetGlobalFlag(197);
            case 172:
            case 213:
            case 214:
            case 225:
            case 226:
            case 255:
            case 256:
            case 345:
            case 346:
                originalScript = "game.global_flags[197] == 0";
                return !GetGlobalFlag(197);
            case 193:
            case 194:
                originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 7";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 7;
            case 201:
            case 202:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 5";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5;
            case 203:
            case 204:
            case 231:
            case 232:
                originalScript = "pc.money_get() >= 50000";
                return pc.GetMoney() >= 50000;
            case 205:
            case 206:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 4 and pc.money_get() >= 10000";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4 && pc.GetMoney() >= 10000;
            case 261:
                originalScript = "(game.party_alignment == LAWFUL_GOOD and pc.skill_level_get(npc,skill_bluff) >= 2) or game.party_alignment != LAWFUL_GOOD";
                return (PartyAlignment == Alignment.LAWFUL_GOOD && pc.GetSkillLevel(npc, SkillId.bluff) >= 2) || PartyAlignment != Alignment.LAWFUL_GOOD;
            case 291:
            case 292:
            case 383:
            case 384:
                originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 2";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 2;
            case 301:
            case 308:
                originalScript = "pc.stat_level_get(stat_strength) >= 18";
                return pc.GetStat(Stat.strength) >= 18;
            case 303:
            case 309:
                originalScript = "pc.stat_level_get(stat_dexterity) >= 18";
                return pc.GetStat(Stat.dexterity) >= 18;
            case 304:
            case 310:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 4";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 4;
            case 305:
                originalScript = "pc.stat_level_get(stat_charisma) >= 18";
                return pc.GetStat(Stat.charisma) >= 18;
            case 402:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 53:
            case 54:
                originalScript = "npc.reaction_adj( pc,-5)";
                npc.AdjustReaction(pc, -5);
                break;
            case 180:
            case 185:
            case 210:
            case 220:
            case 250:
            case 340:
            case 390:
                originalScript = "game.global_flags[31] = 1";
                SetGlobalFlag(31, true);
                break;
            case 181:
            case 186:
            case 211:
            case 212:
            case 221:
            case 222:
            case 223:
            case 224:
            case 361:
            case 391:
            case 392:
            case 395:
                originalScript = "run_off(npc,pc)";
                run_off(npc, pc);
                break;
            case 203:
            case 204:
            case 231:
            case 232:
                originalScript = "pc.money_adj(-50000)";
                pc.AdjustMoney(-50000);
                break;
            case 205:
            case 206:
                originalScript = "pc.money_adj(-10000)";
                pc.AdjustMoney(-10000);
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
            case 22:
            case 31:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 5);
                return true;
            case 81:
            case 82:
            case 261:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 2);
                return true;
            case 83:
            case 84:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                return true;
            case 85:
            case 86:
            case 95:
            case 96:
            case 105:
            case 106:
            case 115:
            case 116:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                return true;
            case 91:
            case 92:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 3);
                return true;
            case 93:
            case 94:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                return true;
            case 193:
            case 194:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 7);
                return true;
            case 201:
            case 202:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 5);
                return true;
            case 205:
            case 206:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4);
                return true;
            case 291:
            case 292:
            case 383:
            case 384:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 2);
                return true;
            case 304:
            case 310:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 4);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}