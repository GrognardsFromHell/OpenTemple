
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog
{
    [DialogScript(62)]
    public class RannosdavlDialog : Rannosdavl, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 4:
                case 34:
                case 35:
                case 54:
                case 55:
                case 72:
                case 73:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 5";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 5;
                case 5:
                case 6:
                case 42:
                case 43:
                case 60:
                case 61:
                case 82:
                case 83:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL && !GetGlobalFlag(67);
                case 7:
                case 8:
                case 74:
                case 75:
                    originalScript = "game.global_flags[40] == 1";
                    return GetGlobalFlag(40);
                case 9:
                case 10:
                case 78:
                case 79:
                    originalScript = "game.global_flags[292] == 1";
                    return GetGlobalFlag(292);
                case 11:
                case 12:
                case 36:
                case 37:
                case 56:
                case 57:
                case 80:
                case 81:
                    originalScript = "game.global_flags[39] == 0";
                    return !GetGlobalFlag(39);
                case 31:
                case 32:
                    originalScript = "game.global_flags[41] == 0";
                    return !GetGlobalFlag(41);
                case 38:
                case 39:
                    originalScript = "game.global_flags[39] == 1";
                    return GetGlobalFlag(39);
                case 40:
                case 41:
                case 58:
                case 59:
                case 76:
                case 77:
                case 294:
                case 295:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL and game.story_state >= 2 and anyone( pc.group_list(), \"has_follower\", 8002 ) == 0 and game.global_flags[371] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL && StoryState >= 2 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8002)) && !GetGlobalFlag(371);
                case 51:
                case 52:
                    originalScript = "game.quests[16].state != qs_completed";
                    return GetQuestState(16) != QuestState.Completed;
                case 111:
                case 112:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 2 and pc.stat_level_get( stat_deity ) == 16";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 2 && pc.GetStat(Stat.deity) == 16;
                case 121:
                case 122:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 141:
                case 142:
                    originalScript = "pc.money_get() >= 18000";
                    return pc.GetMoney() >= 18000;
                case 145:
                case 146:
                case 242:
                case 246:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 3";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 3;
                case 161:
                case 162:
                    originalScript = "pc.money_get() >= 15000";
                    return pc.GetMoney() >= 15000;
                case 171:
                case 172:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 3";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 3;
                case 173:
                case 174:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 5";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 5;
                case 175:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 11";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11;
                case 191:
                case 193:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 5";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 211:
                case 212:
                case 322:
                case 324:
                    originalScript = "game.areas[2] == 0";
                    return !IsAreaKnown(2);
                case 244:
                case 248:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 2 and game.areas[2] == 0";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 2 && !IsAreaKnown(2);
                case 249:
                case 250:
                    originalScript = "game.quests[15].state == qs_completed";
                    return GetQuestState(15) == QuestState.Completed;
                case 321:
                case 323:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 2 and game.areas[2] == 0";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 2 && !IsAreaKnown(2);
                case 325:
                case 326:
                    originalScript = "game.areas[2] == 1";
                    return IsAreaKnown(2);
                case 351:
                case 352:
                    originalScript = "game.global_flags[37] == 0 and game.global_flags[370] == 0";
                    return !GetGlobalFlag(37) && !GetGlobalFlag(370);
                case 353:
                case 354:
                    originalScript = "game.global_flags[37] == 1 and game.global_flags[370] == 0";
                    return GetGlobalFlag(37) && !GetGlobalFlag(370);
                case 355:
                case 356:
                    originalScript = "game.global_flags[370] == 1";
                    return GetGlobalFlag(370);
                case 403:
                case 404:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 4;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 30:
                    originalScript = "game.global_flags[197] = 1";
                    SetGlobalFlag(197, true);
                    break;
                case 50:
                    originalScript = "game.quests[16].state = qs_mentioned";
                    SetQuestState(16, QuestState.Mentioned);
                    break;
                case 51:
                case 52:
                    originalScript = "game.quests[16].state = qs_accepted";
                    SetQuestState(16, QuestState.Accepted);
                    break;
                case 141:
                case 142:
                    originalScript = "game.global_flags[39] = 1; pc.money_adj(-18000)";
                    SetGlobalFlag(39, true);
                    pc.AdjustMoney(-18000);
                    ;
                    break;
                case 161:
                case 162:
                    originalScript = "game.global_flags[39] = 1; pc.money_adj(-15000)";
                    SetGlobalFlag(39, true);
                    pc.AdjustMoney(-15000);
                    ;
                    break;
                case 190:
                case 200:
                case 210:
                    originalScript = "game.global_flags[41] = 1";
                    SetGlobalFlag(41, true);
                    break;
                case 191:
                case 193:
                    originalScript = "pc.money_adj(+20000)";
                    pc.AdjustMoney(+20000);
                    break;
                case 192:
                case 194:
                    originalScript = "pc.money_adj(+2000)";
                    pc.AdjustMoney(+2000);
                    break;
                case 201:
                case 202:
                    originalScript = "npc.item_transfer_to_by_proto(pc,4107)";
                    npc.TransferItemByProtoTo(pc, 4107);
                    break;
                case 215:
                case 270:
                    originalScript = "game.areas[2] = 1; game.story_state = 1";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 216:
                case 217:
                case 271:
                case 272:
                case 421:
                case 422:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 240:
                    originalScript = "game.quests[16].state = qs_completed";
                    SetQuestState(16, QuestState.Completed);
                    break;
                case 241:
                case 245:
                    originalScript = "pc.money_adj(+5000)";
                    pc.AdjustMoney(+5000);
                    break;
                case 242:
                case 246:
                    originalScript = "pc.money_adj(+10000)";
                    pc.AdjustMoney(+10000);
                    break;
                case 320:
                    originalScript = "game.global_flags[67] = 1";
                    SetGlobalFlag(67, true);
                    break;
                case 330:
                    originalScript = "game.areas[2] = 1; game.story_state = 1; game.quests[30].state = qs_completed";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    SetQuestState(30, QuestState.Completed);
                    ;
                    break;
                case 340:
                    originalScript = "game.quests[30].state = qs_completed";
                    SetQuestState(30, QuestState.Completed);
                    break;
                case 351:
                case 352:
                    originalScript = "game.global_flags[370] = 1";
                    SetGlobalFlag(370, true);
                    break;
                case 361:
                case 362:
                    originalScript = "game.global_flags[37] = 1";
                    SetGlobalFlag(37, true);
                    break;
                case 370:
                    originalScript = "game.areas[3] = 1; game.story_state = 3; game.global_flags[371] = 1";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    SetGlobalFlag(371, true);
                    ;
                    break;
                case 371:
                case 372:
                case 441:
                case 442:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 390:
                    originalScript = "game.global_flags[292] = 0";
                    SetGlobalFlag(292, false);
                    break;
                case 430:
                    originalScript = "game.global_flags[371] = 1";
                    SetGlobalFlag(371, true);
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
                case 3:
                case 4:
                case 34:
                case 35:
                case 54:
                case 55:
                case 72:
                case 73:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 5);
                    return true;
                case 111:
                case 112:
                case 321:
                case 323:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 2);
                    return true;
                case 121:
                case 122:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                case 145:
                case 146:
                case 242:
                case 246:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 3);
                    return true;
                case 171:
                case 172:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 3);
                    return true;
                case 173:
                case 174:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 5);
                    return true;
                case 175:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 11);
                    return true;
                case 191:
                case 193:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                case 244:
                case 248:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 2);
                    return true;
                case 403:
                case 404:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
