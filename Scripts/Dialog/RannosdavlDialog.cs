
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

namespace Scripts.Dialog
{
    [DialogScript(62)]
    public class RannosdavlDialog : Rannosdavl, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 4:
                case 32:
                case 33:
                case 52:
                case 53:
                case 72:
                case 73:
                    Trace.Assert(originalScript == "game.global_flags[39] == 0");
                    return !GetGlobalFlag(39);
                case 5:
                case 6:
                case 38:
                case 39:
                case 58:
                case 59:
                case 78:
                case 79:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL && !GetGlobalFlag(67);
                case 7:
                case 8:
                case 74:
                case 75:
                    Trace.Assert(originalScript == "game.global_flags[40] == 1");
                    return GetGlobalFlag(40);
                case 9:
                case 40:
                case 60:
                case 80:
                case 296:
                    Trace.Assert(originalScript == "game.global_flags[41] == 0 and game.quests[17].state == qs_completed");
                    return !GetGlobalFlag(41) && GetQuestState(17) == QuestState.Completed;
                case 10:
                case 21:
                case 41:
                case 61:
                case 81:
                case 297:
                case 2505:
                    Trace.Assert(originalScript == "game.global_flags[277] == 1 and game.global_flags[428] == 0 and game.quests[64].state != qs_completed and game.quests[64].state != qs_botched");
                    return GetGlobalFlag(277) && !GetGlobalFlag(428) && GetQuestState(64) != QuestState.Completed && GetQuestState(64) != QuestState.Botched;
                case 34:
                case 35:
                    Trace.Assert(originalScript == "game.global_flags[39] == 1");
                    return GetGlobalFlag(39);
                case 36:
                case 37:
                case 56:
                case 57:
                case 76:
                case 77:
                case 294:
                case 295:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL and game.story_state >= 2 and game.areas[3] == 0 and anyone( pc.group_list(), \"has_follower\", 8002 ) == 0");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL && StoryState >= 2 && !IsAreaKnown(3) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8002));
                case 54:
                    Trace.Assert(originalScript == "game.quests[16].state != qs_completed and game.quests[15].state == qs_completed");
                    return GetQuestState(16) != QuestState.Completed && GetQuestState(15) == QuestState.Completed;
                case 55:
                    Trace.Assert(originalScript == "game.quests[16].state != qs_completed");
                    return GetQuestState(16) != QuestState.Completed;
                case 91:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 11002 ) and game.quests[64].state != qs_completed");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(11002)) && GetQuestState(64) != QuestState.Completed;
                case 92:
                    Trace.Assert(originalScript == "game.quests[64].state == qs_completed");
                    return GetQuestState(64) == QuestState.Completed;
                case 102:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 103:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 104:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 4 and pc.stat_level_get( stat_deity ) == 16");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 4 && pc.GetStat(Stat.deity) == 16;
                case 121:
                case 122:
                case 403:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "pc.money_get() >= 18000");
                    return pc.GetMoney() >= 18000;
                case 145:
                case 146:
                case 242:
                case 246:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 161:
                case 162:
                    Trace.Assert(originalScript == "pc.money_get() >= 15000");
                    return pc.GetMoney() >= 15000;
                case 171:
                case 172:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 5;
                case 173:
                case 174:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 7;
                case 175:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 14");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 14;
                case 191:
                case 193:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                case 211:
                    Trace.Assert(originalScript == "game.areas[2] == 0 and game.global_flags[37] == 0");
                    return !IsAreaKnown(2) && !GetGlobalFlag(37);
                case 244:
                case 248:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 4 and game.areas[2] == 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 4 && !IsAreaKnown(2);
                case 249:
                case 250:
                    Trace.Assert(originalScript == "game.quests[15].state == qs_completed");
                    return GetQuestState(15) == QuestState.Completed;
                case 321:
                case 323:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 4 and game.areas[2] == 0");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 4 && !IsAreaKnown(2);
                case 322:
                case 324:
                    Trace.Assert(originalScript == "game.areas[2] == 0");
                    return !IsAreaKnown(2);
                case 325:
                case 326:
                    Trace.Assert(originalScript == "game.areas[2] == 1");
                    return IsAreaKnown(2);
                case 351:
                case 352:
                    Trace.Assert(originalScript == "game.global_flags[37] == 0");
                    return !GetGlobalFlag(37);
                case 353:
                case 354:
                    Trace.Assert(originalScript == "game.global_flags[37] == 1");
                    return GetGlobalFlag(37);
                case 402:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 6 and pc.skill_level_get(npc,skill_sense_motive) <= 9");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6 && pc.GetSkillLevel(npc, SkillId.sense_motive) <= 9;
                case 404:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 11002 )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(11002));
                case 441:
                case 442:
                    Trace.Assert(originalScript == "game.global_flags[41] == 0");
                    return !GetGlobalFlag(41);
                case 2501:
                case 2502:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 30:
                    Trace.Assert(originalScript == "game.global_flags[197] = 1");
                    SetGlobalFlag(197, true);
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.quests[16].state = qs_mentioned");
                    SetQuestState(16, QuestState.Mentioned);
                    break;
                case 54:
                case 55:
                    Trace.Assert(originalScript == "game.quests[16].state = qs_accepted");
                    SetQuestState(16, QuestState.Accepted);
                    break;
                case 101:
                case 402:
                    Trace.Assert(originalScript == "game.quests[64].state = qs_completed");
                    SetQuestState(64, QuestState.Completed);
                    break;
                case 102:
                case 103:
                case 104:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.global_flags[39] = 1; pc.money_adj(-18000)");
                    SetGlobalFlag(39, true);
                    pc.AdjustMoney(-18000);
                    ;
                    break;
                case 161:
                case 162:
                    Trace.Assert(originalScript == "game.global_flags[39] = 1; pc.money_adj(-15000)");
                    SetGlobalFlag(39, true);
                    pc.AdjustMoney(-15000);
                    ;
                    break;
                case 191:
                case 193:
                    Trace.Assert(originalScript == "pc.money_adj(+20000); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)");
                    pc.AdjustMoney(+20000);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 192:
                case 194:
                    Trace.Assert(originalScript == "pc.money_adj(+2000); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)");
                    pc.AdjustMoney(+2000);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 201:
                case 202:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto(pc,4107)");
                    npc.TransferItemByProtoTo(pc, 4107);
                    break;
                case 215:
                case 270:
                    Trace.Assert(originalScript == "game.areas[2] = 1; game.story_state = 1");
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
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(2)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.quests[16].state = qs_completed");
                    SetQuestState(16, QuestState.Completed);
                    break;
                case 241:
                case 245:
                    Trace.Assert(originalScript == "pc.money_adj(+5000); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)");
                    pc.AdjustMoney(+5000);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 242:
                case 246:
                    Trace.Assert(originalScript == "pc.money_adj(+10000); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)");
                    pc.AdjustMoney(+10000);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 320:
                    Trace.Assert(originalScript == "game.global_flags[67] = 1");
                    SetGlobalFlag(67, true);
                    break;
                case 330:
                    Trace.Assert(originalScript == "game.areas[2] = 1; game.story_state = 1; game.quests[30].state = qs_completed");
                    MakeAreaKnown(2);
                    StoryState = 1;
                    SetQuestState(30, QuestState.Completed);
                    ;
                    break;
                case 340:
                    Trace.Assert(originalScript == "game.quests[30].state = qs_completed");
                    SetQuestState(30, QuestState.Completed);
                    break;
                case 370:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 371:
                case 372:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 390:
                    Trace.Assert(originalScript == "game.global_flags[428] = 1");
                    SetGlobalFlag(428, true);
                    break;
                case 401:
                    Trace.Assert(originalScript == "game.quests[64].state = qs_botched");
                    SetQuestState(64, QuestState.Botched);
                    break;
                case 441:
                case 442:
                    Trace.Assert(originalScript == "game.global_flags[41] = 1");
                    SetGlobalFlag(41, true);
                    break;
                case 1000:
                    Trace.Assert(originalScript == "game.gloabl_vars[750] = 1; game.global_vars[751] = 1");
                    throw new NotSupportedException("Conversion failed.");
                case 1001:
                    Trace.Assert(originalScript == "switch_to_gremag(npc,pc)");
                    switch_to_gremag(npc, pc);
                    break;
                case 1010:
                    Trace.Assert(originalScript == "game.global_vars[750] = 1; game.global_vars[751] = 1");
                    SetGlobalVar(750, 1);
                    SetGlobalVar(751, 1);
                    ;
                    break;
                case 1012:
                case 1013:
                case 1100:
                case 1101:
                    Trace.Assert(originalScript == "game.global_vars[750] = 1; game.global_vars[751] = 1; npc.attack( pc )");
                    SetGlobalVar(750, 1);
                    SetGlobalVar(751, 1);
                    npc.Attack(pc);
                    ;
                    break;
                case 2000:
                case 2100:
                    Trace.Assert(originalScript == "game.party[0].reputation_remove(23)");
                    PartyLeader.RemoveReputation(23);
                    break;
                case 2012:
                case 2112:
                    Trace.Assert(originalScript == "game.global_flags[883] = 1");
                    SetGlobalFlag(883, true);
                    break;
                case 2024:
                case 2124:
                    Trace.Assert(originalScript == "buff_npc(npc,pc)");
                    buff_npc(npc, pc);
                    break;
                case 2036:
                    Trace.Assert(originalScript == "buff_npc_three(npc,pc)");
                    buff_npc_three(npc, pc);
                    break;
                case 2048:
                case 2148:
                    Trace.Assert(originalScript == "buff_npc_two(npc,pc)");
                    buff_npc_two(npc, pc);
                    break;
                case 2049:
                case 2149:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 2136:
                    Trace.Assert(originalScript == "buff_npc_four(npc,pc)");
                    buff_npc_four(npc, pc);
                    break;
                case 3000:
                case 3100:
                    Trace.Assert(originalScript == "game.global_flags[843] = 1");
                    SetGlobalFlag(843, true);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 111:
                case 112:
                case 321:
                case 323:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 4);
                    return true;
                case 121:
                case 122:
                case 403:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                case 145:
                case 146:
                case 242:
                case 246:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                case 171:
                case 172:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 5);
                    return true;
                case 173:
                case 174:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 7);
                    return true;
                case 175:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 14);
                    return true;
                case 191:
                case 193:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                case 244:
                case 248:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 4);
                    return true;
                case 402:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                    return true;
                case 2501:
                case 2502:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
