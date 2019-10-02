
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
    [DialogScript(338)]
    public class ViscountDialog : Viscount, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "game.global_flags[970] == 0");
                    return !GetGlobalFlag(970);
                case 3:
                    Trace.Assert(originalScript == "game.global_flags[970] == 1");
                    return GetGlobalFlag(970);
                case 4:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 11:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 12:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 13:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 14:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 15:
                    Trace.Assert(originalScript == "game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL;
                case 16:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 17:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 18:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 19:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 21:
                    Trace.Assert(originalScript == "game.quests[75].state == qs_accepted");
                    return GetQuestState(75) == QuestState.Accepted;
                case 22:
                    Trace.Assert(originalScript == "game.global_vars[997] == 1 or game.global_vars[997] == 2 or game.global_vars[997] == 3");
                    return GetGlobalVar(997) == 1 || GetGlobalVar(997) == 2 || GetGlobalVar(997) == 3;
                case 23:
                    Trace.Assert(originalScript == "game.quests[74].state == qs_mentioned");
                    return GetQuestState(74) == QuestState.Mentioned;
                case 24:
                    Trace.Assert(originalScript == "game.quests[74].state == qs_accepted or game.quests[77].state == qs_mentioned or game.quests[77].state == qs_accepted or game.quests[76].state == qs_mentioned or game.quests[76].state == qs_accepted");
                    return GetQuestState(74) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned || GetQuestState(77) == QuestState.Accepted || GetQuestState(76) == QuestState.Mentioned || GetQuestState(76) == QuestState.Accepted;
                case 25:
                    Trace.Assert(originalScript == "game.quests[78].state == qs_mentioned");
                    return GetQuestState(78) == QuestState.Mentioned;
                case 26:
                    Trace.Assert(originalScript == "game.global_vars[993] == 3");
                    return GetGlobalVar(993) == 3;
                case 27:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) or (game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) or (game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) or (game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) || (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) || (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) || (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned);
                case 31:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8;
                case 41:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 51:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male and pc.skill_level_get(npc,skill_intimidate) >= 8");
                    return pc.GetGender() == Gender.Male && pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
                case 52:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_female and pc.skill_level_get(npc,skill_intimidate) >= 8");
                    return pc.GetGender() == Gender.Female && pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
                case 65:
                case 425:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 66:
                case 426:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 67:
                case 427:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 83:
                    Trace.Assert(originalScript == "game.quests[69].state >= qs_mentioned");
                    return GetQuestState(69) >= QuestState.Mentioned;
                case 113:
                case 132:
                    Trace.Assert(originalScript == "game.global_flags[969] == 0");
                    return !GetGlobalFlag(969);
                case 131:
                    Trace.Assert(originalScript == "game.global_flags[968] == 0");
                    return !GetGlobalFlag(968);
                case 141:
                    Trace.Assert(originalScript == "game.global_vars[999] >= 1 and game.global_vars[999] <= 14 and not npc_get(npc,4)");
                    return GetGlobalVar(999) >= 1 && GetGlobalVar(999) <= 14 && !ScriptDaemon.npc_get(npc, 4);
                case 142:
                    Trace.Assert(originalScript == "game.global_vars[999] >= 15 and not npc_get(npc,5)");
                    return GetGlobalVar(999) >= 15 && !ScriptDaemon.npc_get(npc, 5);
                case 143:
                    Trace.Assert(originalScript == "game.global_vars[999] == 0");
                    return GetGlobalVar(999) == 0;
                case 162:
                case 172:
                case 941:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and pc.skill_level_get(npc,skill_sense_motive) <= 10 and game.global_flags[989] == 0 and game.global_flags[946] == 0");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && pc.GetSkillLevel(npc, SkillId.sense_motive) <= 10 && !GetGlobalFlag(989) && !GetGlobalFlag(946);
                case 163:
                case 173:
                case 942:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and pc.skill_level_get(npc,skill_sense_motive) >= 11 and game.global_flags[989] == 0 and game.global_flags[946] == 0");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 11 && !GetGlobalFlag(989) && !GetGlobalFlag(946);
                case 201:
                case 211:
                case 212:
                case 221:
                case 231:
                case 232:
                    Trace.Assert(originalScript == "game.quests[77].state == qs_mentioned");
                    return GetQuestState(77) == QuestState.Mentioned;
                case 202:
                case 213:
                case 214:
                case 222:
                case 233:
                case 234:
                    Trace.Assert(originalScript == "game.quests[77].state == qs_accepted");
                    return GetQuestState(77) == QuestState.Accepted;
                case 241:
                    Trace.Assert(originalScript == "game.global_flags[995] == 1 and game.quests[74].state != qs_completed");
                    return GetGlobalFlag(995) && GetQuestState(74) != QuestState.Completed;
                case 242:
                    Trace.Assert(originalScript == "game.global_flags[995] == 0 and not game.quests[74].state == qs_completed");
                    return !GetGlobalFlag(995) && !(GetQuestState(74) == QuestState.Completed);
                case 251:
                    Trace.Assert(originalScript == "game.quests[74].state == qs_accepted");
                    return GetQuestState(74) == QuestState.Accepted;
                case 252:
                    Trace.Assert(originalScript == "game.quests[69].state == qs_accepted or game.quests[69].state == qs_completed");
                    return GetQuestState(69) == QuestState.Accepted || GetQuestState(69) == QuestState.Completed;
                case 253:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and game.global_flags[946] == 0");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && !GetGlobalFlag(946);
                case 254:
                    Trace.Assert(originalScript == "(game.quests[76].state == qs_mentioned or game.quests[76].state == qs_accepted) and game.global_flags[989] == 1");
                    return (GetQuestState(76) == QuestState.Mentioned || GetQuestState(76) == QuestState.Accepted) && GetGlobalFlag(989);
                case 261:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 262:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 263:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 271:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 281:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 291:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 331:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 341:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
                case 351:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male and pc.skill_level_get(npc,skill_intimidate) >= 12");
                    return pc.GetGender() == Gender.Male && pc.GetSkillLevel(npc, SkillId.intimidate) >= 12;
                case 352:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_female and pc.skill_level_get(npc,skill_intimidate) >= 12");
                    return pc.GetGender() == Gender.Female && pc.GetSkillLevel(npc, SkillId.intimidate) >= 12;
                case 361:
                    Trace.Assert(originalScript == "pc.money_get() >= 50000");
                    return pc.GetMoney() >= 50000;
                case 362:
                    Trace.Assert(originalScript == "pc.money_get() <= 49900");
                    return pc.GetMoney() <= 49900;
                case 421:
                    Trace.Assert(originalScript == "game.global_flags[147] == 1");
                    return GetGlobalFlag(147);
                case 422:
                    Trace.Assert(originalScript == "game.global_flags[146] == 1");
                    return GetGlobalFlag(146);
                case 423:
                    Trace.Assert(originalScript == "game.global_flags[189] == 1");
                    return GetGlobalFlag(189);
                case 424:
                    Trace.Assert(originalScript == "game.global_flags[327] == 1");
                    return GetGlobalFlag(327);
                case 431:
                case 441:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 432:
                case 442:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 433:
                case 443:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 491:
                    Trace.Assert(originalScript == "(game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == NEUTRAL_GOOD)");
                    return (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.NEUTRAL_GOOD);
                case 492:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_EVIL)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 493:
                    Trace.Assert(originalScript == "(game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 661:
                case 701:
                case 781:
                case 791:
                case 801:
                case 811:
                    Trace.Assert(originalScript == "(game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD)");
                    return (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD);
                case 662:
                case 702:
                case 792:
                case 802:
                case 812:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)");
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 663:
                case 703:
                case 783:
                case 793:
                case 803:
                case 813:
                    Trace.Assert(originalScript == "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL)");
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 761:
                    Trace.Assert(originalScript == "game.global_vars[995] == 0");
                    return GetGlobalVar(995) == 0;
                case 762:
                    Trace.Assert(originalScript == "game.global_vars[995] == 1");
                    return GetGlobalVar(995) == 1;
                case 763:
                    Trace.Assert(originalScript == "game.global_vars[995] == 2");
                    return GetGlobalVar(995) == 2;
                case 764:
                    Trace.Assert(originalScript == "game.global_vars[995] == 3");
                    return GetGlobalVar(995) == 3;
                case 765:
                    Trace.Assert(originalScript == "game.global_vars[995] == 4");
                    return GetGlobalVar(995) == 4;
                case 766:
                    Trace.Assert(originalScript == "game.global_vars[995] == 5");
                    return GetGlobalVar(995) == 5;
                case 767:
                    Trace.Assert(originalScript == "game.global_vars[995] == 6");
                    return GetGlobalVar(995) == 6;
                case 768:
                    Trace.Assert(originalScript == "game.global_vars[995] == 7");
                    return GetGlobalVar(995) == 7;
                case 769:
                    Trace.Assert(originalScript == "game.global_vars[995] == 8");
                    return GetGlobalVar(995) == 8;
                case 770:
                    Trace.Assert(originalScript == "game.global_vars[995] == 9");
                    return GetGlobalVar(995) == 9;
                case 771:
                    Trace.Assert(originalScript == "game.global_vars[995] == 10");
                    return GetGlobalVar(995) == 10;
                case 772:
                    Trace.Assert(originalScript == "game.global_vars[995] == 11");
                    return GetGlobalVar(995) == 11;
                case 782:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment ==  CHAOTIC_NEUTRAL)");
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 851:
                    Trace.Assert(originalScript == "game.global_vars[997] == 1");
                    return GetGlobalVar(997) == 1;
                case 852:
                    Trace.Assert(originalScript == "game.global_vars[997] == 2 or game.global_vars[997] == 3");
                    return GetGlobalVar(997) == 2 || GetGlobalVar(997) == 3;
                case 943:
                    Trace.Assert(originalScript == "(game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) and game.global_flags[973] == 0 and not npc_get(npc,1)");
                    return (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) && !GetGlobalFlag(973) && !ScriptDaemon.npc_get(npc, 1);
                case 944:
                    Trace.Assert(originalScript == "(game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,2)");
                    return (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 2);
                case 945:
                    Trace.Assert(originalScript == "(game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,3)");
                    return (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 3);
                case 981:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) <= 10");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) <= 10;
                case 982:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 11");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 11;
                case 991:
                    Trace.Assert(originalScript == "game.global_vars[948] == 2");
                    return GetGlobalVar(948) == 2;
                case 992:
                    Trace.Assert(originalScript == "game.global_vars[947] == 2");
                    return GetGlobalVar(947) == 2;
                case 993:
                    Trace.Assert(originalScript == "game.global_vars[946] == 2");
                    return GetGlobalVar(946) == 2;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    Trace.Assert(originalScript == "game.global_flags[970] = 1");
                    SetGlobalFlag(970, true);
                    break;
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                    Trace.Assert(originalScript == "schedule_bandits_1(npc,pc)");
                    schedule_bandits_1(npc, pc);
                    break;
                case 23:
                case 81:
                    Trace.Assert(originalScript == "game.quests[74].state = qs_accepted");
                    SetQuestState(74, QuestState.Accepted);
                    break;
                case 25:
                case 491:
                case 492:
                case 493:
                    Trace.Assert(originalScript == "game.quests[78].state = qs_accepted");
                    SetQuestState(78, QuestState.Accepted);
                    break;
                case 26:
                    Trace.Assert(originalScript == "game.global_vars[993] = 4");
                    SetGlobalVar(993, 4);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.quests[74].state = qs_mentioned");
                    SetQuestState(74, QuestState.Mentioned);
                    break;
                case 80:
                    Trace.Assert(originalScript == "game.global_flags[985] = 1");
                    SetGlobalFlag(985, true);
                    break;
                case 130:
                    Trace.Assert(originalScript == "game.global_flags[986] = 1");
                    SetGlobalFlag(986, true);
                    break;
                case 141:
                    Trace.Assert(originalScript == "npc_set(npc,4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 142:
                    Trace.Assert(originalScript == "npc_set(npc,5)");
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 162:
                case 163:
                case 172:
                case 173:
                case 253:
                case 941:
                case 942:
                case 981:
                case 982:
                    Trace.Assert(originalScript == "game.global_flags[946] = 1");
                    SetGlobalFlag(946, true);
                    break;
                case 201:
                case 221:
                    Trace.Assert(originalScript == "distribute_verbobonc_uniform(npc,pc); game.quests[76].state = qs_accepted; ditch_captains(npc,pc)");
                    distribute_verbobonc_uniform(npc, pc);
                    SetQuestState(76, QuestState.Accepted);
                    ditch_captains(npc, pc);
                    ;
                    break;
                case 202:
                case 222:
                    Trace.Assert(originalScript == "distribute_verbobonc_uniform(npc,pc); game.quests[76].state = qs_accepted; game.quests[77].state = qs_botched; ditch_captains(npc,pc)");
                    distribute_verbobonc_uniform(npc, pc);
                    SetQuestState(76, QuestState.Accepted);
                    SetQuestState(77, QuestState.Botched);
                    ditch_captains(npc, pc);
                    ;
                    break;
                case 211:
                case 231:
                    Trace.Assert(originalScript == "game.quests[76].state = qs_accepted; ditch_captains(npc,pc)");
                    SetQuestState(76, QuestState.Accepted);
                    ditch_captains(npc, pc);
                    ;
                    break;
                case 212:
                case 232:
                    Trace.Assert(originalScript == "game.quests[76].state = qs_mentioned");
                    SetQuestState(76, QuestState.Mentioned);
                    break;
                case 213:
                case 233:
                    Trace.Assert(originalScript == "game.quests[76].state = qs_accepted; game.quests[77].state = qs_botched; ditch_captains(npc,pc)");
                    SetQuestState(76, QuestState.Accepted);
                    SetQuestState(77, QuestState.Botched);
                    ditch_captains(npc, pc);
                    ;
                    break;
                case 214:
                case 234:
                    Trace.Assert(originalScript == "game.quests[76].state = qs_mentioned; game.quests[77].state = qs_botched");
                    SetQuestState(76, QuestState.Mentioned);
                    SetQuestState(77, QuestState.Botched);
                    ;
                    break;
                case 241:
                    Trace.Assert(originalScript == "game.quests[74].state = qs_completed");
                    SetQuestState(74, QuestState.Completed);
                    break;
                case 254:
                    Trace.Assert(originalScript == "game.quests[76].state = qs_completed");
                    SetQuestState(76, QuestState.Completed);
                    break;
                case 301:
                case 311:
                case 321:
                case 332:
                case 342:
                case 353:
                case 362:
                case 391:
                case 412:
                    Trace.Assert(originalScript == "game.quests[75].state = qs_botched");
                    SetQuestState(75, QuestState.Botched);
                    break;
                case 361:
                    Trace.Assert(originalScript == "pc.money_adj(-50000); game.quests[75].state = qs_completed");
                    pc.AdjustMoney(-50000);
                    SetQuestState(75, QuestState.Completed);
                    ;
                    break;
                case 371:
                case 381:
                    Trace.Assert(originalScript == "game.quests[75].state = qs_completed");
                    SetQuestState(75, QuestState.Completed);
                    break;
                case 430:
                    Trace.Assert(originalScript == "game.quests[112].state = qs_completed");
                    SetQuestState(112, QuestState.Completed);
                    break;
                case 450:
                    Trace.Assert(originalScript == "game.global_vars[993] = 1; game.quests[111].state = qs_completed");
                    SetGlobalVar(993, 1);
                    SetQuestState(111, QuestState.Completed);
                    ;
                    break;
                case 460:
                    Trace.Assert(originalScript == "game.quests[78].state = qs_mentioned");
                    SetQuestState(78, QuestState.Mentioned);
                    break;
                case 500:
                    Trace.Assert(originalScript == "game.global_vars[993] = 2");
                    SetGlobalVar(993, 2);
                    break;
                case 501:
                    Trace.Assert(originalScript == "slavers_movie_setup(npc,pc)");
                    slavers_movie_setup(npc, pc);
                    break;
                case 630:
                    Trace.Assert(originalScript == "game.global_vars[993] = 6");
                    SetGlobalVar(993, 6);
                    break;
                case 700:
                    Trace.Assert(originalScript == "pc.money_adj(1000000); game.quests[78].state = qs_completed");
                    pc.AdjustMoney(1000000);
                    SetQuestState(78, QuestState.Completed);
                    ;
                    break;
                case 710:
                    Trace.Assert(originalScript == "game.global_vars[993] = 10");
                    SetGlobalVar(993, 10);
                    break;
                case 750:
                    Trace.Assert(originalScript == "game.party[0].reputation_add(39)");
                    PartyLeader.AddReputation(39);
                    break;
                case 780:
                    Trace.Assert(originalScript == "pc.money_adj(750000); game.quests[78].state = qs_completed");
                    pc.AdjustMoney(750000);
                    SetQuestState(78, QuestState.Completed);
                    ;
                    break;
                case 790:
                    Trace.Assert(originalScript == "pc.money_adj(500000); game.quests[78].state = qs_completed");
                    pc.AdjustMoney(500000);
                    SetQuestState(78, QuestState.Completed);
                    ;
                    break;
                case 800:
                    Trace.Assert(originalScript == "pc.money_adj(250000); game.quests[78].state = qs_completed");
                    pc.AdjustMoney(250000);
                    SetQuestState(78, QuestState.Completed);
                    ;
                    break;
                case 810:
                    Trace.Assert(originalScript == "game.quests[78].state = qs_completed");
                    SetQuestState(78, QuestState.Completed);
                    break;
                case 830:
                    Trace.Assert(originalScript == "game.party[0].reputation_add(50)");
                    PartyLeader.AddReputation(50);
                    break;
                case 860:
                case 870:
                    Trace.Assert(originalScript == "game.global_vars[997] = 4");
                    SetGlobalVar(997, 4);
                    break;
                case 900:
                    Trace.Assert(originalScript == "game.quests[78].state = qs_botched");
                    SetQuestState(78, QuestState.Botched);
                    break;
                case 901:
                    Trace.Assert(originalScript == "game.party[0].reputation_add(41)");
                    PartyLeader.AddReputation(41);
                    break;
                case 911:
                case 921:
                case 932:
                    Trace.Assert(originalScript == "npc.attack( pc ); game.global_flags[863] = 1");
                    npc.Attack(pc);
                    SetGlobalFlag(863, true);
                    ;
                    break;
                case 943:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 944:
                    Trace.Assert(originalScript == "npc_set(npc,2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 945:
                    Trace.Assert(originalScript == "npc_set(npc,3)");
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 991:
                    Trace.Assert(originalScript == "switch_to_captain( npc, pc, 280)");
                    switch_to_captain(npc, pc, 280);
                    break;
                case 992:
                    Trace.Assert(originalScript == "switch_to_captain( npc, pc, 450)");
                    switch_to_captain(npc, pc, 450);
                    break;
                case 993:
                    Trace.Assert(originalScript == "switch_to_captain( npc, pc, 470)");
                    switch_to_captain(npc, pc, 470);
                    break;
                case 1041:
                    Trace.Assert(originalScript == "game.global_vars[501] = 1; game.quests[107].state = qs_accepted");
                    SetGlobalVar(501, 1);
                    SetQuestState(107, QuestState.Accepted);
                    ;
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
                case 31:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                case 41:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 8);
                    return true;
                case 51:
                case 52:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 163:
                case 173:
                case 942:
                case 982:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 11);
                    return true;
                case 271:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                case 281:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                case 291:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                case 331:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 12);
                    return true;
                case 341:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                    return true;
                case 351:
                case 352:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
