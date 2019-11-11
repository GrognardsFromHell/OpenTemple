
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
    [DialogScript(12)]
    public class JarooDialog : Jaroo, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 18:
                case 19:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 6:
                case 7:
                    originalScript = "npc.has_met(pc) and game.party_alignment == TRUE_NEUTRAL and game.global_flags[18] == 0";
                    return npc.HasMet(pc) && PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(18);
                case 8:
                case 9:
                    originalScript = "game.story_state >= 2 and game.party_alignment == TRUE_NEUTRAL and game.global_flags[19] == 0 and game.global_vars[562] == 4";
                    return StoryState >= 2 && PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(19) && GetGlobalVar(562) == 4;
                case 10:
                case 11:
                case 89:
                case 90:
                    originalScript = "game.global_flags[17] == 1 and game.quests[5].state != qs_completed";
                    return GetGlobalFlag(17) && GetQuestState(5) != QuestState.Completed;
                case 12:
                case 13:
                    originalScript = "game.quests[99].state == qs_botched";
                    return GetQuestState(99) == QuestState.Botched;
                case 14:
                case 15:
                    originalScript = "pc.item_find( 12900 ) == OBJ_HANDLE_NULL and npc.has_met(pc) and game.quests[99].state == qs_accepted";
                    return pc.FindItemByName(12900) == null && npc.HasMet(pc) && GetQuestState(99) == QuestState.Accepted;
                case 16:
                case 17:
                    originalScript = "pc.item_find( 12900 ) != OBJ_HANDLE_NULL and npc.has_met(pc) and game.quests[99].state == qs_accepted";
                    return pc.FindItemByName(12900) != null && npc.HasMet(pc) && GetQuestState(99) == QuestState.Accepted;
                case 27:
                case 28:
                case 93:
                case 94:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL and game.global_flags[18] == 0";
                    return PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(18);
                case 41:
                case 517:
                case 519:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                case 42:
                    originalScript = "pc.money_get() >= 2000";
                    return pc.GetMoney() >= 2000;
                case 43:
                    originalScript = "pc.money_get() >= 500";
                    return pc.GetMoney() >= 500;
                case 44:
                    originalScript = "pc.money_get() >= 100";
                    return pc.GetMoney() >= 100;
                case 81:
                case 82:
                    originalScript = "pc.item_find( 3000 ) != OBJ_HANDLE_NULL or pc.item_find(5800) != OBJ_HANDLE_NULL";
                    return pc.FindItemByName(3000) != null || pc.FindItemByName(5800) != null;
                case 87:
                case 88:
                    originalScript = "game.global_flags[12] == 1 and game.quests[8].state == qs_accepted and game.global_flags[13] == 0";
                    return GetGlobalFlag(12) && GetQuestState(8) == QuestState.Accepted && !GetGlobalFlag(13);
                case 95:
                    originalScript = "game.quests[2].state == qs_mentioned";
                    return GetQuestState(2) == QuestState.Mentioned;
                case 96:
                    originalScript = "game.quests[2].state == qs_accepted";
                    return GetQuestState(2) == QuestState.Accepted;
                case 97:
                    originalScript = "game.quests[2].state == qs_mentioned or game.quests[2].state == qs_accepted";
                    return GetQuestState(2) == QuestState.Mentioned || GetQuestState(2) == QuestState.Accepted;
                case 101:
                    originalScript = "game.party_alignment & ALIGNMENT_EVIL != 0";
                    return PartyAlignment.IsEvil();
                case 102:
                    originalScript = "game.party_alignment & ALIGNMENT_GOOD != 0";
                    return PartyAlignment.IsGood();
                case 103:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 161:
                case 162:
                    originalScript = "pc.item_find( 3000 ) != OBJ_HANDLE_NULL";
                    return pc.FindItemByName(3000) != null;
                case 163:
                case 164:
                    originalScript = "pc.item_find(5800) != OBJ_HANDLE_NULL and game.global_flags[3] == 0";
                    return pc.FindItemByName(5800) != null && !GetGlobalFlag(3);
                case 191:
                case 193:
                    originalScript = "game.quests[3].state == qs_accepted";
                    return GetQuestState(3) == QuestState.Accepted;
                case 192:
                case 194:
                    originalScript = "game.quests[3].state <= qs_mentioned";
                    return GetQuestState(3) <= QuestState.Mentioned;
                case 204:
                case 205:
                    originalScript = "( game.quests[6].state == qs_mentioned or game.quests[6].state == qs_accepted ) and game.global_flags[10] == 1";
                    return (GetQuestState(6) == QuestState.Mentioned || GetQuestState(6) == QuestState.Accepted) && GetGlobalFlag(10);
                case 206:
                case 207:
                    originalScript = "game.quests[5].state == qs_accepted";
                    return GetQuestState(5) == QuestState.Accepted;
                case 261:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 353:
                case 354:
                    originalScript = "game.areas[3] == 1";
                    return IsAreaKnown(3);
                case 355:
                case 356:
                    originalScript = "game.areas[3] == 0";
                    return !IsAreaKnown(3);
                case 372:
                case 373:
                case 374:
                case 376:
                case 377:
                case 378:
                case 551:
                case 552:
                    originalScript = "game.global_vars[2] >= 100";
                    return GetGlobalVar(2) >= 100;
                case 379:
                case 380:
                case 381:
                case 382:
                case 383:
                case 384:
                case 553:
                case 554:
                    originalScript = "game.global_vars[2] <= 99";
                    return GetGlobalVar(2) <= 99;
                case 385:
                case 386:
                    originalScript = "game.story_state >= 1";
                    return StoryState >= 1;
                case 401:
                case 403:
                case 421:
                case 423:
                    originalScript = "pc.money_get() >= 21000";
                    return pc.GetMoney() >= 21000;
                case 437:
                case 439:
                    originalScript = "pc.money_get() >= 7000";
                    return pc.GetMoney() >= 7000;
                case 463:
                case 465:
                    originalScript = "pc.money_get() >= 30000";
                    return pc.GetMoney() >= 30000;
                case 481:
                case 483:
                case 501:
                case 503:
                    originalScript = "pc.money_get() >= 25000";
                    return pc.GetMoney() >= 25000;
                case 543:
                case 545:
                    originalScript = "pc.money_get() >= 35000";
                    return pc.GetMoney() >= 35000;
                case 563:
                case 565:
                    originalScript = "pc.money_get() >= 22500";
                    return pc.GetMoney() >= 22500;
                case 573:
                case 575:
                    originalScript = "pc.money_get() >= 27500";
                    return pc.GetMoney() >= 27500;
                case 581:
                    originalScript = "game.global_vars[501] == 2";
                    return GetGlobalVar(501) == 2;
                case 591:
                    originalScript = "game.quests[97].state == qs_accepted";
                    return GetQuestState(97) == QuestState.Accepted;
                case 592:
                    originalScript = "game.quests[97].state != qs_accepted";
                    return GetQuestState(97) != QuestState.Accepted;
                case 601:
                case 602:
                case 701:
                case 702:
                    originalScript = "game.global_flags[855] == 0";
                    return !GetGlobalFlag(855);
                case 603:
                case 604:
                case 703:
                case 704:
                    originalScript = "game.global_flags[855] == 1";
                    return GetGlobalFlag(855);
                case 641:
                    originalScript = "game.global_vars[523] <= 1";
                    return GetGlobalVar(523) <= 1;
                case 642:
                    originalScript = "game.global_vars[523] >= 2";
                    return GetGlobalVar(523) >= 2;
                case 643:
                    originalScript = "game.global_vars[524] <= 3";
                    return GetGlobalVar(524) <= 3;
                case 644:
                    originalScript = "game.global_vars[524] >= 4";
                    return GetGlobalVar(524) >= 4;
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
                case 580:
                    originalScript = "game.global_flags[1] = 1";
                    SetGlobalFlag(1, true);
                    break;
                case 6:
                case 7:
                case 93:
                case 94:
                    originalScript = "game.global_flags[18] = 1; game.global_flags[67] = 1";
                    SetGlobalFlag(18, true);
                    SetGlobalFlag(67, true);
                    ;
                    break;
                case 8:
                case 9:
                    originalScript = "game.global_flags[19] = 1";
                    SetGlobalFlag(19, true);
                    break;
                case 16:
                case 17:
                    originalScript = "party_transfer_to( npc, 12900 )";
                    Utilities.party_transfer_to(npc, 12900);
                    break;
                case 27:
                case 28:
                    originalScript = "game.global_flags[18] = 1";
                    SetGlobalFlag(18, true);
                    break;
                case 41:
                    originalScript = "game.global_vars[2] = game.global_vars[2] + 100;  pc.money_adj(-10000)";
                    SetGlobalVar(2, GetGlobalVar(2) + 100);
                    pc.AdjustMoney(-10000);
                    ;
                    break;
                case 42:
                    originalScript = "game.global_vars[2] = game.global_vars[2] + 20; pc.money_adj(-2000)";
                    SetGlobalVar(2, GetGlobalVar(2) + 20);
                    pc.AdjustMoney(-2000);
                    ;
                    break;
                case 43:
                    originalScript = "game.global_vars[2] = game.global_vars[2] + 5; pc.money_adj(-500)";
                    SetGlobalVar(2, GetGlobalVar(2) + 5);
                    pc.AdjustMoney(-500);
                    ;
                    break;
                case 44:
                    originalScript = "game.global_vars[2] = game.global_vars[2] + 1; pc.money_adj(-100)";
                    SetGlobalVar(2, GetGlobalVar(2) + 1);
                    pc.AdjustMoney(-100);
                    ;
                    break;
                case 100:
                    originalScript = "game.quests[4].state = qs_mentioned";
                    SetQuestState(4, QuestState.Mentioned);
                    break;
                case 140:
                case 150:
                    originalScript = "game.global_flags[23] = 1";
                    SetGlobalFlag(23, true);
                    break;
                case 170:
                    originalScript = "game.global_flags[3] = 1";
                    SetGlobalFlag(3, true);
                    break;
                case 230:
                    originalScript = "game.global_flags[8] = 1";
                    SetGlobalFlag(8, true);
                    break;
                case 270:
                case 310:
                    originalScript = "game.global_flags[13] = 1";
                    SetGlobalFlag(13, true);
                    break;
                case 323:
                    originalScript = "game.areas[2] = 1; game.story_state = 1; game.quests[72].state = qs_mentioned";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    SetQuestState(72, QuestState.Mentioned);
                    ;
                    break;
                case 324:
                case 325:
                    originalScript = "game.quests[72].state = qs_accepted; game.global_vars[562] = 4; game.worldmap_travel_by_dialog(2)";
                    SetQuestState(72, QuestState.Accepted);
                    SetGlobalVar(562, 4);
                    // FIXME: worldmap_travel_by_dialog;
                    ;
                    break;
                case 326:
                case 327:
                    originalScript = "game.quests[72].state = qs_accepted; game.global_vars[562] = 4";
                    SetQuestState(72, QuestState.Accepted);
                    SetGlobalVar(562, 4);
                    ;
                    break;
                case 336:
                    originalScript = "game.quests[5].state = qs_completed";
                    SetQuestState(5, QuestState.Completed);
                    break;
                case 346:
                    originalScript = "game.quests[26].state = qs_completed; game.quests[72].state = qs_completed; game.global_vars[562] = 6";
                    SetQuestState(26, QuestState.Completed);
                    SetQuestState(72, QuestState.Completed);
                    SetGlobalVar(562, 6);
                    ;
                    break;
                case 358:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 359:
                case 360:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 390:
                    originalScript = "game.picker( npc, spell_remove_disease, should_heal_disease_on, [ 450, 370, 400 ] )";
                    // FIXME: picker;
                    break;
                case 401:
                case 403:
                    originalScript = "pc.money_adj(-21000); npc.cast_spell( spell_remove_disease, picker_obj )";
                    pc.AdjustMoney(-21000);
                    npc.CastSpell(WellKnownSpells.RemoveDisease, PickedObject);
                    ;
                    break;
                case 405:
                case 485:
                case 720:
                    originalScript = "npc.spells_pending_to_memorized()";
                    npc.PendingSpellsToMemorized();
                    break;
                case 410:
                    originalScript = "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 450, 370, 420 ] )";
                    // FIXME: picker;
                    break;
                case 421:
                case 423:
                    originalScript = "pc.money_adj(-21000); npc.cast_spell( spell_neutralize_poison, picker_obj )";
                    pc.AdjustMoney(-21000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 434:
                    originalScript = "game.picker( npc, spell_cure_light_wounds, should_heal_hp_on, [ 450, 370, 436 ] )";
                    // FIXME: picker;
                    break;
                case 437:
                case 439:
                    originalScript = "pc.money_adj(-7000); npc.cast_spell( spell_cure_light_wounds, picker_obj )";
                    pc.AdjustMoney(-7000);
                    npc.CastSpell(WellKnownSpells.CureLightWounds, PickedObject);
                    ;
                    break;
                case 460:
                    originalScript = "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 450, 370, 462 ] )";
                    // FIXME: picker;
                    break;
                case 463:
                case 465:
                    originalScript = "pc.money_adj(-30000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )";
                    pc.AdjustMoney(-30000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 470:
                    originalScript = "game.picker( npc, spell_remove_disease, should_heal_disease_on, [ 450, 370, 480 ] )";
                    // FIXME: picker;
                    break;
                case 481:
                case 483:
                    originalScript = "pc.money_adj(-25000); npc.cast_spell( spell_remove_disease, picker_obj )";
                    pc.AdjustMoney(-25000);
                    npc.CastSpell(WellKnownSpells.RemoveDisease, PickedObject);
                    ;
                    break;
                case 490:
                    originalScript = "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 450, 370, 500 ] )";
                    // FIXME: picker;
                    break;
                case 501:
                case 503:
                    originalScript = "pc.money_adj(-25000); npc.cast_spell( spell_neutralize_poison, picker_obj )";
                    pc.AdjustMoney(-25000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 514:
                    originalScript = "game.picker( npc, spell_cure_light_wounds, should_heal_hp_on, [ 450, 370, 516 ] )";
                    // FIXME: picker;
                    break;
                case 517:
                case 519:
                    originalScript = "pc.money_adj(-10000); npc.cast_spell( spell_cure_light_wounds, picker_obj )";
                    pc.AdjustMoney(-10000);
                    npc.CastSpell(WellKnownSpells.CureLightWounds, PickedObject);
                    ;
                    break;
                case 540:
                    originalScript = "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 450, 370, 542 ] )";
                    // FIXME: picker;
                    break;
                case 543:
                case 545:
                    originalScript = "pc.money_adj(-35000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )";
                    pc.AdjustMoney(-35000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 560:
                case 570:
                    originalScript = "game.picker( npc, spell_reincarnation, should_resurrect_on, [ 450, 370, 562 ] )";
                    // FIXME: picker;
                    break;
                case 563:
                case 565:
                    originalScript = "pc.money_adj(-22500); npc.cast_spell( spell_reincarnation, picker_obj )";
                    pc.AdjustMoney(-22500);
                    npc.CastSpell(WellKnownSpells.Reincarnation, PickedObject);
                    ;
                    break;
                case 573:
                case 575:
                    originalScript = "pc.money_adj(-27500); npc.cast_spell( spell_reincarnation, picker_obj )";
                    pc.AdjustMoney(-27500);
                    npc.CastSpell(WellKnownSpells.Reincarnation, PickedObject);
                    ;
                    break;
                case 651:
                case 652:
                    originalScript = "game.global_flags[855] = 1";
                    SetGlobalFlag(855, true);
                    break;
                case 660:
                    originalScript = "game.picker( npc, spell_reincarnation, should_resurrect_on, [ 690, 640, 710 ] )";
                    // FIXME: picker;
                    break;
                case 683:
                case 684:
                    originalScript = "amii_dies_due_to_dont_give_a_damn_dialog(npc,pc)";
                    amii_dies_due_to_dont_give_a_damn_dialog(npc, pc);
                    break;
                case 710:
                    originalScript = "game.global_vars[523] = game.global_vars[523] + 1";
                    SetGlobalVar(523, GetGlobalVar(523) + 1);
                    break;
                case 711:
                    originalScript = "npc.cast_spell( spell_reincarnation, picker_obj )";
                    npc.CastSpell(WellKnownSpells.Reincarnation, PickedObject);
                    break;
                case 730:
                    originalScript = "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 690, 640, 740 ] )";
                    // FIXME: picker;
                    break;
                case 740:
                    originalScript = "game.global_vars[524] = game.global_vars[524] + 1";
                    SetGlobalVar(524, GetGlobalVar(524) + 1);
                    break;
                case 741:
                    originalScript = "npc.cast_spell( spell_cure_serious_wounds, picker_obj )";
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    break;
                case 861:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
                    break;
                case 971:
                case 980:
                case 981:
                    originalScript = "game.global_vars[455] |= 2**7";
                    SetGlobalVar(455, GetGlobalVar(455) | 0x80);
                    break;
                case 1000:
                    originalScript = "game.picker( npc, spell_reincarnation, should_clear_spell_on, [ 1045, 370, 1562 ] )";
                    // FIXME: picker;
                    break;
                case 1562:
                    originalScript = "game.global_vars[753] = picker_obj.stat_level_get(stat_experience); kill_picked(picker_obj, npc)";
                    SetGlobalVar(753, PickedObject.GetStat(Stat.experience));
                    kill_picked(PickedObject, npc);
                    ;
                    break;
                case 1575:
                    originalScript = "picker_obj.stat_base_set(stat_experience, game.global_vars[753]); npc.spells_pending_to_memorized()";
                    PickedObject.SetBaseStat(Stat.experience, GetGlobalVar(753));
                    npc.PendingSpellsToMemorized();
                    ;
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
                case 261:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
