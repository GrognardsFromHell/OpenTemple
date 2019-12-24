
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
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

namespace Scripts.Dialog
{
    [DialogScript(61)]
    public class GremagDialog : Gremag, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                case 6:
                case 52:
                case 53:
                case 504:
                case 505:
                    originalScript = "game.global_flags[40] == 1";
                    return GetGlobalFlag(40);
                case 7:
                case 11:
                case 12:
                case 34:
                case 46:
                case 54:
                case 264:
                case 506:
                    originalScript = "game.global_flags[41] == 0 and game.quests[17].state == qs_completed";
                    return !GetGlobalFlag(41) && GetQuestState(17) == QuestState.Completed;
                case 8:
                case 21:
                case 35:
                case 47:
                case 55:
                case 267:
                case 507:
                case 2505:
                    originalScript = "game.global_flags[277] == 1 and game.global_flags[428] == 0 and game.quests[64].state != qs_completed and game.quests[64].state != qs_botched";
                    return GetGlobalFlag(277) && !GetGlobalFlag(428) && GetQuestState(64) != QuestState.Completed && GetQuestState(64) != QuestState.Botched;
                case 32:
                case 33:
                case 42:
                case 43:
                    originalScript = "game.global_flags[39] == 0";
                    return !GetGlobalFlag(39);
                case 44:
                case 45:
                    originalScript = "game.quests[16].state != qs_completed";
                    return GetQuestState(16) != QuestState.Completed;
                case 61:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 11002 ) and game.quests[64].state != qs_completed";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(11002)) && GetQuestState(64) != QuestState.Completed;
                case 62:
                    originalScript = "game.quests[64].state == qs_completed";
                    return GetQuestState(64) == QuestState.Completed;
                case 72:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 73:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 74:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 81:
                case 82:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 4;
                case 111:
                case 112:
                    originalScript = "pc.money_get() >= 20000";
                    return pc.GetMoney() >= 20000;
                case 115:
                case 116:
                case 212:
                case 216:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 5";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 131:
                case 132:
                    originalScript = "pc.money_get() >= 15000";
                    return pc.GetMoney() >= 15000;
                case 141:
                case 142:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 4;
                case 143:
                case 144:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 6;
                case 145:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 161:
                case 163:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                case 181:
                    originalScript = "game.areas[2] == 0 and game.global_flags[37] == 0";
                    return !IsAreaKnown(2) && !GetGlobalFlag(37);
                case 214:
                case 218:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 4 and game.areas[2] == 0";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 4 && !IsAreaKnown(2);
                case 219:
                case 220:
                    originalScript = "game.quests[15].state == qs_completed";
                    return GetQuestState(15) == QuestState.Completed;
                case 265:
                case 266:
                    originalScript = "game.global_flags[835] == 1 or game.global_flags[837] == 1 and game.global_flags[37] == 0";
                    return GetGlobalFlag(835) || GetGlobalFlag(837) && !GetGlobalFlag(37);
                case 333:
                case 334:
                case 391:
                case 392:
                    originalScript = "game.global_flags[297] == 1";
                    return GetGlobalFlag(297);
                case 353:
                case 354:
                case 401:
                case 402:
                    originalScript = "game.global_flags[298] == 1";
                    return GetGlobalFlag(298);
                case 363:
                case 364:
                case 411:
                case 412:
                    originalScript = "game.global_flags[299] == 1";
                    return GetGlobalFlag(299);
                case 382:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 6 and pc.skill_level_get(npc,skill_sense_motive) <= 9";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6 && pc.GetSkillLevel(npc, SkillId.sense_motive) <= 9;
                case 383:
                case 431:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 384:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 11002 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(11002));
                case 2501:
                case 2502:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
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
                case 11:
                case 12:
                    originalScript = "game.global_flags[41] = 1";
                    SetGlobalFlag(41, true);
                    break;
                case 40:
                    originalScript = "game.quests[16].state = qs_mentioned";
                    SetQuestState(16, QuestState.Mentioned);
                    break;
                case 44:
                case 45:
                    originalScript = "game.quests[16].state = qs_accepted";
                    SetQuestState(16, QuestState.Accepted);
                    break;
                case 71:
                case 382:
                    originalScript = "game.quests[64].state = qs_completed";
                    SetQuestState(64, QuestState.Completed);
                    break;
                case 72:
                case 73:
                case 74:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 111:
                case 112:
                    originalScript = "game.global_flags[39] = 1; pc.money_adj(-20000)";
                    SetGlobalFlag(39, true);
                    pc.AdjustMoney(-20000);
                    ;
                    break;
                case 131:
                case 132:
                    originalScript = "game.global_flags[39] = 1; pc.money_adj(-15000)";
                    SetGlobalFlag(39, true);
                    pc.AdjustMoney(-15000);
                    ;
                    break;
                case 161:
                case 163:
                    originalScript = "pc.money_adj(+20000); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                    pc.AdjustMoney(+20000);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 162:
                case 164:
                case 211:
                case 215:
                    originalScript = "pc.money_adj(+5000); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                    pc.AdjustMoney(+5000);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 171:
                case 172:
                    originalScript = "npc.item_transfer_to_by_proto(pc,4107); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                    npc.TransferItemByProtoTo(pc, 4107);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 185:
                case 240:
                    originalScript = "game.areas[2] = 1; game.story_state = 1";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 186:
                case 187:
                case 241:
                case 242:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 210:
                    originalScript = "game.quests[16].state = qs_completed";
                    SetQuestState(16, QuestState.Completed);
                    break;
                case 212:
                case 216:
                    originalScript = "pc.money_adj(+10000); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                    pc.AdjustMoney(+10000);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 290:
                    originalScript = "game.global_flags[293] = 1";
                    SetGlobalFlag(293, true);
                    break;
                case 330:
                    originalScript = "game.quests[91].state = qs_mentioned";
                    SetQuestState(91, QuestState.Mentioned);
                    break;
                case 331:
                case 332:
                    originalScript = "game.global_flags[294] = 1; game.quests[91].state = qs_accepted";
                    SetGlobalFlag(294, true);
                    SetQuestState(91, QuestState.Accepted);
                    ;
                    break;
                case 350:
                    originalScript = "game.quests[91].state = qs_completed; game.quests[92].state = qs_mentioned";
                    SetQuestState(91, QuestState.Completed);
                    SetQuestState(92, QuestState.Mentioned);
                    ;
                    break;
                case 351:
                case 352:
                    originalScript = "game.global_flags[295] = 1; game.quests[92].state = qs_accepted";
                    SetGlobalFlag(295, true);
                    SetQuestState(92, QuestState.Accepted);
                    ;
                    break;
                case 360:
                    originalScript = "game.quests[92].state = qs_completed; game.quests[93].state = qs_mentioned";
                    SetQuestState(92, QuestState.Completed);
                    SetQuestState(93, QuestState.Mentioned);
                    ;
                    break;
                case 361:
                    originalScript = "game.global_flags[296] = 1; game.quests[93].state = qs_accepted";
                    SetGlobalFlag(296, true);
                    SetQuestState(93, QuestState.Accepted);
                    ;
                    break;
                case 362:
                    originalScript = "game.global_flags[296] = 1; game.quests[92].state = qs_accepted";
                    SetGlobalFlag(296, true);
                    SetQuestState(92, QuestState.Accepted);
                    ;
                    break;
                case 370:
                    originalScript = "game.party[0].reputation_add( 24 ); game.quests[93].state = qs_completed";
                    PartyLeader.AddReputation(24);
                    SetQuestState(93, QuestState.Completed);
                    ;
                    break;
                case 380:
                    originalScript = "game.global_flags[428] = 1";
                    SetGlobalFlag(428, true);
                    break;
                case 381:
                    originalScript = "game.quests[64].state = qs_botched";
                    SetQuestState(64, QuestState.Botched);
                    break;
                case 1000:
                    originalScript = "game.gloabl_vars[750] = 1; game.global_vars[751] = 1";
                    throw new NotSupportedException("Conversion failed.");
                case 1001:
                    originalScript = "switch_to_rannos(npc,pc)";
                    switch_to_rannos(npc, pc);
                    break;
                case 1010:
                    originalScript = "game.global_vars[750] = 1; game.global_vars[751] = 1";
                    SetGlobalVar(750, 1);
                    SetGlobalVar(751, 1);
                    ;
                    break;
                case 1012:
                case 1013:
                case 1100:
                case 1101:
                    originalScript = "game.global_vars[750] = 1; game.global_vars[751] = 1; npc.attack( pc )";
                    SetGlobalVar(750, 1);
                    SetGlobalVar(751, 1);
                    npc.Attack(pc);
                    ;
                    break;
                case 2100:
                    originalScript = "game.party[0].reputation_remove(23)";
                    PartyLeader.RemoveReputation(23);
                    break;
                case 2112:
                    originalScript = "game.global_flags[883] = 1";
                    SetGlobalFlag(883, true);
                    break;
                case 2124:
                    originalScript = "buff_npc(npc,pc)";
                    buff_npc(npc, pc);
                    break;
                case 2136:
                    originalScript = "buff_npc_four(npc,pc)";
                    buff_npc_four(npc, pc);
                    break;
                case 2148:
                    originalScript = "buff_npc_two(npc,pc)";
                    buff_npc_two(npc, pc);
                    break;
                case 2149:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 3000:
                case 3100:
                    originalScript = "game.global_flags[843] = 1";
                    SetGlobalFlag(843, true);
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
                case 81:
                case 82:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 4);
                    return true;
                case 115:
                case 116:
                case 212:
                case 216:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                case 141:
                case 142:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 4);
                    return true;
                case 143:
                case 144:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 6);
                    return true;
                case 145:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 12);
                    return true;
                case 161:
                case 163:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                case 214:
                case 218:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 4);
                    return true;
                case 382:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                    return true;
                case 383:
                case 431:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
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
